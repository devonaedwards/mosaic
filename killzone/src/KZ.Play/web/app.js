// KILL ZONE - a real-time strategy video game.
// The renderer and the input layer. Host-agnostic: it knows four URLs and
// nothing else about what is serving them.
//
// The one architectural rule here, from docs/SCALE.md's correction: this file
// is a pure view transform. Everything it receives is in real metres and
// everything it draws is in pixels, and the only thing standing between the two
// is `cam.ppm` - pixels per metre. There is no compression factor baked in
// anywhere, which is what lets the same build open on a phone and a desktop at
// different zoom defaults rather than as different games.

'use strict';

// ---------------------------------------------------------------------------
// State

var S = null;            // /api/static, fetched once
var V = null;            // the latest /api/state
var since = 0;           // event sequence watermark
var events = [];         // rolling narration

var cam = { cx: 14400, cy: 9600, ppm: 0.04 };
var sel = null;          // {kind:'own'|'foe', h:handleValue}
var armed = null;        // a hangar defId waiting for a target
var setPadMode = false;
var showSensors = true;
var pad = { x: 10320, y: 9360 };
var pendingMoves = [];   // launches made at bare ground, waiting for a handle
var knownHandles = {};

var cv = document.getElementById('cv');
var ctx = cv.getContext('2d');
var terrainCanvas = null;

// Tile classes, in TileClass order: Open Road Forest PowerLine Rubble Water Impassable
var TILE_COLOUR = ['#131a14', '#2b2823', '#16301c', '#2d2134', '#2a2a2a', '#0e1c33', '#000000'];
var PIP_COLOUR = { Green: '#46d17a', Amber: '#ffb02e', Black: '#8a8f94' };
var CHANNEL_LETTER = { Optical: 'O', Thermal: 'T', Acoustic: 'A', Radar: 'R', Esm: 'E', held: '·' };

// What kind of track is behind a contact, which is the one thing the player has
// to know before deciding whether an interceptor is worth launching at it. A
// radar track measures the target's velocity and buys a computed meeting point;
// an optical one is inferred from image scale and sends the interceptor short.
// A bearing is one listener hearing a transmitter: a direction with no range on
// it, which nothing on this side is allowed to shoot at and which is drawn as an
// arc rather than a ring because that is honestly all of it that is known.
var TRACK_MARK = { Radar: '◎', Optical: '○', Bearing: '⌒', None: '·' };
var TRACK_COLOUR = { Radar: '#46d17a', Optical: '#ffb02e', Bearing: '#6f8fd0', None: '#8a8f94' };

// Anything of theirs that is off the ground and not a structure. This is the
// only category of contact that needs answering inside the next ninety seconds,
// so it gets sorted to the top of the list and drawn with a ring round it.
function isInbound(c) { return c.layer > 0 && !c.structure && !c.salvage; }

// ---------------------------------------------------------------------------
// The view transform. These four functions are the whole of it.

function sx(x) { return (x - cam.cx) * cam.ppm + cv.width / (2 * dpr()); }
function sy(y) { return (y - cam.cy) * cam.ppm + cv.height / (2 * dpr()); }
function wx(px) { return (px - cv.width / (2 * dpr())) / cam.ppm + cam.cx; }
function wy(py) { return (py - cv.height / (2 * dpr())) / cam.ppm + cam.cy; }

function dpr() { return window.devicePixelRatio || 1; }

function resize() {
  var r = cv.getBoundingClientRect();
  cv.width = Math.max(1, Math.round(r.width * dpr()));
  cv.height = Math.max(1, Math.round(r.height * dpr()));
  ctx.setTransform(dpr(), 0, 0, dpr(), 0, 0);
}
window.addEventListener('resize', function () { resize(); draw(); });

function viewW() { return cv.width / dpr(); }
function viewH() { return cv.height / dpr(); }

function fitTo(cx, cy, widthMetres) {
  cam.cx = cx; cam.cy = cy;
  cam.ppm = viewW() / widthMetres;
  clampCam();
}

function clampCam() {
  if (!S) return;
  // Never zoom out past three map widths, never in past 2 px per metre. Both
  // ends are arbitrary and both exist so a fat-fingered pinch cannot lose the
  // map entirely.
  var minPpm = viewW() / (S.widthMetres * 3);
  cam.ppm = Math.min(2, Math.max(minPpm, cam.ppm));
  cam.cx = Math.max(-S.widthMetres * 0.2, Math.min(S.widthMetres * 1.2, cam.cx));
  cam.cy = Math.max(-S.heightMetres * 0.2, Math.min(S.heightMetres * 1.2, cam.cy));
}

// ---------------------------------------------------------------------------
// Network

function getJson(url) {
  return fetch(url, { cache: 'no-store' }).then(function (r) { return r.json(); });
}

function post(url, body) {
  return fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body)
  });
}

function command(c) { return post('/api/command', c); }
function control(c) { return post('/api/control', c); }

function poll() {
  getJson('/api/state?since=' + since).then(function (v) {
    V = v;
    since = v.eventSeq;
    for (var i = 0; i < v.events.length; i++) events.push(v.events[i]);
    if (events.length > 120) events = events.slice(events.length - 120);
    resolvePendingMoves();
    updatePanels();
  }).catch(function () { /* the host is restarting; the next poll will do */ });
}

// A launch aimed at bare ground cannot carry its destination: the airframe does
// not exist yet, so there is no handle to give a move order to. So we launch,
// wait one snapshot, find the airframe that appeared on the pad, and send the
// move as an ordinary second command. Both are real commands in the replay,
// which is why this is done here rather than by inventing a compound order.
function resolvePendingMoves() {
  if (!V) return;
  var fresh = [];
  for (var i = 0; i < V.units.length; i++) {
    var u = V.units[i];
    if (!knownHandles[u.h]) { fresh.push(u); knownHandles[u.h] = true; }
  }
  while (pendingMoves.length && fresh.length) {
    var want = pendingMoves.shift();
    var u = fresh.shift();
    command({ kind: 'move', subject: u.h, x: Math.round(want.x), y: Math.round(want.y) });
  }
  if (pendingMoves.length > 4) pendingMoves = [];
}

// ---------------------------------------------------------------------------
// Drawing

function buildTerrain() {
  terrainCanvas = document.createElement('canvas');
  terrainCanvas.width = S.tilesX;
  terrainCanvas.height = S.tilesY;
  var tc = terrainCanvas.getContext('2d');
  var img = tc.createImageData(S.tilesX, S.tilesY);
  for (var ty = 0; ty < S.tilesY; ty++) {
    var row = S.tiles[ty];
    for (var tx = 0; tx < S.tilesX; tx++) {
      var c = TILE_COLOUR[row.charCodeAt(tx) - 48] || '#000';
      var o = (ty * S.tilesX + tx) * 4;
      img.data[o] = parseInt(c.substr(1, 2), 16);
      img.data[o + 1] = parseInt(c.substr(3, 2), 16);
      img.data[o + 2] = parseInt(c.substr(5, 2), 16);
      img.data[o + 3] = 255;
    }
  }
  tc.putImageData(img, 0, 0);
}

function draw() {
  if (!S) return;
  ctx.setTransform(dpr(), 0, 0, dpr(), 0, 0);
  ctx.fillStyle = '#05070a';
  ctx.fillRect(0, 0, viewW(), viewH());

  // Terrain. One image, scaled by the view transform and nothing else.
  ctx.imageSmoothingEnabled = false;
  ctx.drawImage(terrainCanvas, sx(0), sy(0),
                S.widthMetres * cam.ppm, S.heightMetres * cam.ppm);

  drawGrid();
  drawBorder();
  if (!V) return;

  drawJamming();
  if (showSensors) drawSensors();
  drawTethers();
  drawPad();
  drawUnits();
  drawContacts();
  drawSelection();
  drawLegend();
}

// A kilometre grid, so distance on this map is readable without a ruler. It is
// also the only thing on screen that tells you the zoom changed.
function drawGrid() {
  var step = 1000;
  while (step * cam.ppm < 48) step *= 2;
  ctx.strokeStyle = 'rgba(120,140,155,0.10)';
  ctx.lineWidth = 1;
  ctx.beginPath();
  for (var x = 0; x <= S.widthMetres; x += step) { ctx.moveTo(sx(x), sy(0)); ctx.lineTo(sx(x), sy(S.heightMetres)); }
  for (var y = 0; y <= S.heightMetres; y += step) { ctx.moveTo(sx(0), sy(y)); ctx.lineTo(sx(S.widthMetres), sy(y)); }
  ctx.stroke();
}

// The political border, which is not the front line. West of it your satellite
// links work; east of it they do not, whoever holds the ground.
function drawBorder() {
  ctx.strokeStyle = 'rgba(255,220,120,0.45)';
  ctx.setLineDash([6, 5]);
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(sx(S.borderMetres), sy(0));
  ctx.lineTo(sx(S.borderMetres), sy(S.heightMetres));
  ctx.stroke();
  ctx.setLineDash([]);
  if (cam.ppm > 0.012) {
    ctx.fillStyle = 'rgba(255,220,120,0.7)';
    ctx.fillText('BORDER', sx(S.borderMetres) + 4, sy(0) + 12);
  }
}

function drawJamming() {
  for (var i = 0; i < V.jamming.length; i++) {
    var j = V.jamming[i];
    var r = j.radius * cam.ppm;
    ctx.beginPath();
    ctx.arc(sx(j.x), sy(j.y), r, 0, 6.2832);
    ctx.fillStyle = j.mine ? 'rgba(80,200,255,0.06)' : 'rgba(255,90,79,0.10)';
    ctx.fill();
    ctx.strokeStyle = j.mine ? 'rgba(80,200,255,0.35)' : 'rgba(255,90,79,0.45)';
    ctx.setLineDash([3, 4]);
    ctx.stroke();
    ctx.setLineDash([]);
  }
}

// Your own sensors: nominal reach, and where the head is actually pointed. A
// directional sensor that is looking the other way is the difference between
// seeing something and not, and nothing else on this screen shows it.
function drawSensors() {
  for (var i = 0; i < V.units.length; i++) {
    var u = V.units[i];
    if (!u.sensor) continue;
    var best = Math.max(u.sensor.optical, u.sensor.thermal, u.sensor.acoustic,
                        u.sensor.radar, u.sensor.esm);
    if (best <= 0) continue;
    var px = sx(u.x), py = sy(u.y), r = best * cam.ppm;
    if (r < 3) continue;
    ctx.strokeStyle = 'rgba(79,209,255,0.16)';
    ctx.lineWidth = 1;
    if (u.sensor.arc >= 360) {
      ctx.beginPath(); ctx.arc(px, py, r, 0, 6.2832); ctx.stroke();
    } else {
      var half = u.sensor.arc * Math.PI / 360;
      var f = u.sensor.facing * Math.PI / 180;
      ctx.beginPath();
      ctx.moveTo(px, py);
      ctx.arc(px, py, r, f - half, f + half);
      ctx.closePath();
      ctx.fillStyle = 'rgba(79,209,255,0.05)';
      ctx.fill();
      ctx.stroke();
    }
  }
}

function drawTethers() {
  ctx.strokeStyle = 'rgba(230,240,255,0.55)';
  ctx.lineWidth = 1;
  for (var i = 0; i < V.tethers.length; i++) {
    var p = V.tethers[i].pts;
    if (p.length < 4) continue;
    ctx.beginPath();
    ctx.moveTo(sx(p[0]), sy(p[1]));
    for (var n = 2; n < p.length; n += 2) ctx.lineTo(sx(p[n]), sy(p[n + 1]));
    ctx.stroke();
  }
}

function drawPad() {
  var px = sx(pad.x), py = sy(pad.y);
  ctx.strokeStyle = '#4fd1ff';
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(px - 7, py); ctx.lineTo(px + 7, py);
  ctx.moveTo(px, py - 7); ctx.lineTo(px, py + 7);
  ctx.stroke();
  ctx.fillStyle = 'rgba(79,209,255,0.8)';
  if (cam.ppm > 0.01) ctx.fillText('PAD', px + 9, py + 4);
}

function drawUnits() {
  for (var i = 0; i < V.units.length; i++) {
    var u = V.units[i];
    var px = sx(u.x), py = sy(u.y);

    // The control line: who is actually flying this thing. A drone parented on
    // a relay mast dies with the mast, and a pip alone never says that.
    if (u.link && u.link.px !== undefined) {
      ctx.strokeStyle = 'rgba(79,209,255,0.22)';
      ctx.lineWidth = 1;
      ctx.beginPath(); ctx.moveTo(px, py); ctx.lineTo(sx(u.link.px), sy(u.link.py)); ctx.stroke();
    }
    // Where it has been told to go.
    if (u.mover && u.mover.hasOrder) {
      ctx.strokeStyle = 'rgba(79,209,255,0.35)';
      ctx.setLineDash([2, 4]);
      ctx.beginPath(); ctx.moveTo(px, py); ctx.lineTo(sx(u.mover.ox), sy(u.mover.oy)); ctx.stroke();
      ctx.setLineDash([]);
    }

    ctx.fillStyle = '#4fd1ff';
    ctx.strokeStyle = '#4fd1ff';
    ctx.lineWidth = 1;
    if (u.structure) {
      var s = 5;
      ctx.strokeRect(px - s, py - s, s * 2, s * 2);
      ctx.fillRect(px - 2, py - 2, 4, 4);
    } else if (u.layer > 0) {
      // Airborne: a chevron on its heading, so a flight reads as going
      // somewhere rather than sitting somewhere.
      var a = u.yaw * Math.PI / 180;
      ctx.beginPath();
      ctx.moveTo(px + Math.cos(a) * 6, py + Math.sin(a) * 6);
      ctx.lineTo(px + Math.cos(a + 2.5) * 5, py + Math.sin(a + 2.5) * 5);
      ctx.lineTo(px + Math.cos(a - 2.5) * 5, py + Math.sin(a - 2.5) * 5);
      ctx.closePath();
      ctx.fill();
    } else {
      ctx.beginPath(); ctx.arc(px, py, 4, 0, 6.2832); ctx.stroke();
    }

    // Link pip. Green / amber / black is the whole design thesis in one dot.
    if (u.link) {
      ctx.fillStyle = PIP_COLOUR[u.link.pip] || '#888';
      ctx.fillRect(px + 6, py - 9, 4, 4);
    }
    // Damage: a bar only once it has taken some, so an undamaged map is quiet.
    if (u.hp < u.hpMax) {
      var frac = Math.max(0, u.hp / u.hpMax);
      ctx.fillStyle = '#2a3238'; ctx.fillRect(px - 8, py + 8, 16, 2);
      ctx.fillStyle = frac > 0.5 ? '#46d17a' : '#ffb02e';
      ctx.fillRect(px - 8, py + 8, 16 * frac, 2);
    }
    // Semantic zoom: far out, a structure is worth naming and a drone is not,
    // because a dozen overlapping "Fiber FPV Team" labels is less information
    // than none. Close in, everything is named.
    if (cam.ppm > (u.structure ? 0.02 : 0.09) || (sel && sel.h === u.h)) {
      ctx.fillStyle = 'rgba(190,215,230,0.75)';
      ctx.fillText(u.name, px + 8, py + 4);
    }
  }
}

// Everything the other side owns arrives here or not at all. There is no
// dimmed silhouette for something out of contact: a contact you have lost is
// simply gone from the screen, which is the point.
function drawContacts() {
  for (var i = 0; i < V.contacts.length; i++) {
    var c = V.contacts[i];
    var px = sx(c.x), py = sy(c.y);

    // The line back to the sensor holding it. This is the single most useful
    // thing the map can say: it is not "there is a tank", it is "the designator
    // team is the reason you know about that tank".
    if (c.sx !== undefined && cam.ppm > 0.006) {
      ctx.strokeStyle = 'rgba(255,90,79,0.13)';
      ctx.lineWidth = 1;
      ctx.beginPath(); ctx.moveTo(px, py); ctx.lineTo(sx(c.sx), sy(c.sy)); ctx.stroke();
    }

    ctx.strokeStyle = c.salvage ? '#8a8f94' : '#ff5a4f';
    ctx.fillStyle = ctx.strokeStyle;
    ctx.lineWidth = 1;
    ctx.beginPath();
    if (c.structure) {
      ctx.rect(px - 5, py - 5, 10, 10);
      ctx.stroke();
      ctx.fillRect(px - 2, py - 2, 4, 4);
    } else {
      ctx.moveTo(px, py - 6); ctx.lineTo(px + 6, py); ctx.lineTo(px, py + 6); ctx.lineTo(px - 6, py);
      ctx.closePath();
      ctx.stroke();
      if (c.layer > 0) ctx.fill();
    }
    // A threat ring. A one-pixel diamond among thirty other one-pixel diamonds
    // is not an alert, and an interception you did not notice in time is an
    // interception you could not have made.
    if (isInbound(c)) {
      ctx.strokeStyle = TRACK_COLOUR[c.track] || '#8a8f94';
      ctx.globalAlpha = 0.55;
      ctx.beginPath(); ctx.arc(px, py, 13, 0, 6.2832); ctx.stroke();
      ctx.globalAlpha = 1;
    }

    ctx.fillStyle = 'rgba(255,140,130,0.9)';
    ctx.fillText(CHANNEL_LETTER[c.channel] || '?', px - 3, py - 8);

    // Damage on a contact is the only confirmation the player gets that a
    // strike landed at all: there is no hit event in the simulation, only a
    // kill one, and four drones into a tank that is still alive has to look
    // different from four drones into empty ground.
    if (c.hp < c.hpMax) {
      var cf = Math.max(0, c.hp / c.hpMax);
      ctx.fillStyle = '#382a28'; ctx.fillRect(px - 8, py + 8, 16, 2);
      ctx.fillStyle = '#ff5a4f'; ctx.fillRect(px - 8, py + 8, 16 * cf, 2);
    }
    if (cam.ppm > (c.structure ? 0.02 : 0.09) || (sel && sel.h === c.h)) {
      ctx.fillStyle = 'rgba(255,170,160,0.8)';
      ctx.fillText(c.name, px + 8, py + 4);
    }
  }
}

function drawSelection() {
  var e = selected();
  if (!e) return;
  ctx.strokeStyle = sel.kind === 'own' ? '#eaf7ff' : '#ffd0cc';
  ctx.lineWidth = 1;
  ctx.beginPath(); ctx.arc(sx(e.x), sy(e.y), 11, 0, 6.2832); ctx.stroke();

  // A selected unit's weapon envelope, if it has one.
  if (e.weapon && e.weapon.range > 0) {
    ctx.strokeStyle = 'rgba(255,176,46,0.35)';
    ctx.setLineDash([2, 3]);
    ctx.beginPath(); ctx.arc(sx(e.x), sy(e.y), e.weapon.range * cam.ppm, 0, 6.2832); ctx.stroke();
    ctx.setLineDash([]);
  }
}

function drawLegend() {
  var el = document.getElementById('legend');
  var km = (viewW() / cam.ppm / 1000).toFixed(1);
  el.innerHTML =
    'view ' + km + ' km wide &nbsp; ' + (cam.ppm * 1000).toFixed(1) + ' px/km<br>' +
    '<span class="own">■ yours</span> &nbsp; <span class="foe">◆ contact</span>' +
    ' &nbsp; letter = channel (O T A R E)<br>' +
    'drag pan · wheel/pinch zoom · click select · right-click order<br>' +
    'card then target = sortie &nbsp; space = pause &nbsp; e = radiate/quiet';
}

// ---------------------------------------------------------------------------
// Panels

function fmtClock(s) { return Math.floor(s / 60) + ':' + ('0' + (s % 60)).slice(-2); }

function updatePanels() {
  if (!V) return;
  document.getElementById('clock').textContent = fmtClock(V.playSeconds);
  document.getElementById('mat').textContent = V.you.materiel;
  document.getElementById('tp').textContent = V.you.taskingPoints;
  document.getElementById('crews').textContent =
    V.you.crewsReady + ' rdy / ' + V.you.crewsFlying + ' up / ' + V.you.crewsTotal;
  document.getElementById('cond').textContent =
    V.phase + ' · ' + V.weather + ' · ' + V.ground;

  // The one number that says whether there is anything to do right now.
  var inbound = V.contacts.filter(isInbound);
  var alert = document.getElementById('alert');
  if (inbound.length) {
    var cued = inbound.filter(function (c) { return c.track === 'Radar'; }).length;
    alert.textContent = '▲ ' + inbound.length + ' airborne' + (cued ? ' · ' + cued + ' on radar' : '');
    alert.className = 'cell alert on';
  } else {
    alert.textContent = '';
    alert.className = 'cell alert';
  }

  // The ground threat, and the reason this scenario can be lost at all: an
  // armoured column bounds west while it has a jammer over it and shells the
  // command post when it arrives. A player who never reads the map still gets
  // told, because eight play-minutes of warning that nothing draws is no
  // warning - and the "no cover" state is told too, because that is the half
  // that says the answer is working.
  var ground = document.getElementById('ground');
  if (V.threat && V.threat.any) {
    var km = (V.threat.metres / 1000).toFixed(1);
    ground.textContent = '▬ ' + V.threat.name + ' ' + km + ' km'
      + (V.threat.jammed ? ' · jammed' : ' · clear');
    ground.className = 'cell ground on' + (V.threat.metres <= 4000 ? ' urgent' : '');
  } else {
    ground.textContent = '';
    ground.className = 'cell ground';
  }

  var obj = [];
  for (var i = 0; i < V.objectives.length; i++) {
    obj.push((V.objectives[i].alive ? '□ ' : '■ ') + V.objectives[i].name);
  }
  document.getElementById('obj').textContent = obj.join('  ');

  document.getElementById('playbtn').textContent = V.paused ? '▶ play' : '❚❚ pause';

  // Radiate or stay quiet, for the structures that can choose. The button is
  // only there when the selection can actually do it, because an order you
  // cannot give should not be a greyed-out button you keep trying.
  var emitBtn = document.getElementById('emitbtn');
  var es = selected();
  if (sel && sel.kind === 'own' && es && es.emitting !== undefined) {
    emitBtn.style.display = '';
    emitBtn.textContent = es.emitting ? '◉ radiating' : '○ quiet';
    emitBtn.classList.toggle('on', !!es.emitting);
  } else {
    emitBtn.style.display = 'none';
  }

  var banner = document.getElementById('banner');
  if (V.outcome !== 'playing') {
    banner.style.display = 'block';
    banner.textContent = V.outcome === 'won'
      ? 'SECTOR HELD — every objective down'
      : 'SECTOR LOST — your command post is gone';
  } else banner.style.display = 'none';

  setHtml('objlist', obj.map(function (o) {
    return '<div class="row" style="cursor:default"><span>' + o + '</span></div>';
  }).join(''));
  setHtml('contacts', contactsHtml());
  setHtml('sorties', sortiesHtml());
  setHtml('log', logHtml());
  drawHangar();
}

var lastHtml = {};
function setHtml(id, h) {
  if (lastHtml[id] === h) return;
  lastHtml[id] = h;
  document.getElementById(id).innerHTML = h;
}

// Airborne first and nearest first inside that, because the list is a queue of
// decisions and the ones with a clock on them go at the top. A ground contact
// four kilometres away will still be there in a minute; an FPV will not.
function contactsHtml() {
  var h = '';
  var sorted = V.contacts.slice().sort(function (a, b) {
    if (isInbound(a) !== isInbound(b)) return isInbound(a) ? -1 : 1;
    return range(a) - range(b);
  });
  for (var i = 0; i < sorted.length; i++) {
    var c = sorted[i];
    var d = Math.round(range(c) / 100) / 10;
    var air = isInbound(c);
    h += '<div class="row foe' + (air ? ' inbound' : '') + '" data-h="' + c.h + '" data-k="foe">' +
         '<span>' + (air ? '▲' : '◆') + ' ' + c.name + '</span><span class="r">' +
         '<span style="color:' + (TRACK_COLOUR[c.track] || '#8a8f94') + '">' +
         (TRACK_MARK[c.track] || '·') + '</span> ' +
         (CHANNEL_LETTER[c.channel] || '?') + ' ' + d + ' km</span></div>';
  }
  if (!V.contacts.length) h = '<div class="row" style="cursor:default"><span>nothing in contact</span></div>';
  return h;
}

function sortiesHtml() {
  var h = '';
  for (var i = 0; i < V.units.length; i++) {
    var u = V.units[i];
    if (!u.sortie) continue;
    var pip = u.link ? u.link.pip : '—';
    h += '<div class="row own" data-h="' + u.h + '" data-k="own">' +
         '<span>▸ ' + u.name + '</span><span class="r" style="color:' +
         (PIP_COLOUR[pip] || '#888') + '">' + u.sortie.phase + ' ' + pip + '</span></div>';
  }
  if (!h) h = '<div class="row" style="cursor:default"><span>nothing in the air</span></div>';
  return h;
}

function logHtml() {
  var h = '';
  for (var i = events.length - 1; i >= 0 && i > events.length - 40; i--) {
    var e = events[i];
    var hot = /Kill|Destroyed|Black|Lost|Missed|Rejected|Refused/.test(e.kind);
    h += '<div class="' + (hot ? 'hot' : '') + '">' + fmtClock(e.t) + ' ' + e.text + '</div>';
  }
  return h;
}

// Range from the launch pad, because the only question a contact list has to
// answer before anything else is "can I reach that from where I launch".
function range(c) { return Math.hypot(c.x - pad.x, c.y - pad.y); }

// Rebuilt only when it would actually look different. Ten innerHTML rewrites a
// second detaches the very element the player is clicking on, which is a real
// input bug and not a performance nicety.
var lastHangarHtml = '';

function drawHangar() {
  var bar = document.getElementById('hangar');
  var h = '';
  for (var i = 0; i < S.hangar.length; i++) {
    var d = S.hangar[i];
    var cant = V && (V.you.materiel < d.cost || (d.crew && V.you.crewsReady <= 0));
    h += '<div class="card' + (armed === d.defId ? ' armed' : '') + (cant ? ' cant' : '') +
         '" data-def="' + d.defId + '">' +
         '<div class="n">' + d.name + '</div>' +
         '<div class="s">◆' + d.cost + ' · ' + d.link + (d.oneWay ? ' · one-way' : '') + '</div>' +
         '<div class="s">' + d.speed + ' m/s' + (d.nightOnly ? ' · night' : '') + '</div>' +
         '</div>';
  }
  if (h === lastHangarHtml) return;
  lastHangarHtml = h;
  bar.innerHTML = h;
}

// ---------------------------------------------------------------------------
// Picking and input

function selected() {
  if (!sel || !V) return null;
  var list = sel.kind === 'own' ? V.units : V.contacts;
  for (var i = 0; i < list.length; i++) if (list[i].h === sel.h) return list[i];
  return null;
}

function pick(mx, my) {
  if (!V) return null;
  var bestD = 18 * 18, best = null;
  function test(list, kind) {
    for (var i = 0; i < list.length; i++) {
      var e = list[i];
      var dx = sx(e.x) - mx, dy = sy(e.y) - my;
      var d = dx * dx + dy * dy;
      if (d < bestD) { bestD = d; best = { kind: kind, h: e.h, e: e }; }
    }
  }
  test(V.contacts, 'foe');
  test(V.units, 'own');
  return best;
}

var drag = null;
var pointers = {};
var pinchDist = 0;

cv.addEventListener('pointerdown', function (ev) {
  cv.setPointerCapture(ev.pointerId);
  pointers[ev.pointerId] = { x: ev.clientX, y: ev.clientY };
  if (Object.keys(pointers).length === 2) {
    var p = Object.keys(pointers).map(function (k) { return pointers[k]; });
    pinchDist = Math.hypot(p[0].x - p[1].x, p[0].y - p[1].y);
    drag = null;
    return;
  }
  var r = cv.getBoundingClientRect();
  drag = { x: ev.clientX, y: ev.clientY, mx: ev.clientX - r.left, my: ev.clientY - r.top,
           moved: false, button: ev.button };
});

cv.addEventListener('pointermove', function (ev) {
  if (!pointers[ev.pointerId]) return;
  pointers[ev.pointerId] = { x: ev.clientX, y: ev.clientY };
  var ids = Object.keys(pointers);
  if (ids.length === 2) {
    var p = ids.map(function (k) { return pointers[k]; });
    var d = Math.hypot(p[0].x - p[1].x, p[0].y - p[1].y);
    if (pinchDist > 0) { cam.ppm *= d / pinchDist; clampCam(); draw(); }
    pinchDist = d;
    return;
  }
  if (!drag) return;
  var dx = ev.clientX - drag.x, dy = ev.clientY - drag.y;
  if (Math.abs(dx) + Math.abs(dy) > 4) drag.moved = true;
  if (drag.moved) {
    cam.cx -= dx / cam.ppm; cam.cy -= dy / cam.ppm;
    clampCam(); draw();
    drag.x = ev.clientX; drag.y = ev.clientY;
  }
});

cv.addEventListener('pointerup', function (ev) {
  delete pointers[ev.pointerId];
  if (Object.keys(pointers).length < 2) pinchDist = 0;
  if (!drag) return;
  if (!drag.moved) onClick(drag.mx, drag.my, drag.button);
  drag = null;
});
cv.addEventListener('pointercancel', function (ev) { delete pointers[ev.pointerId]; drag = null; });
cv.addEventListener('contextmenu', function (ev) { ev.preventDefault(); });

cv.addEventListener('wheel', function (ev) {
  ev.preventDefault();
  var r = cv.getBoundingClientRect();
  var mx = ev.clientX - r.left, my = ev.clientY - r.top;
  var before = { x: wx(mx), y: wy(my) };
  cam.ppm *= Math.exp(-ev.deltaY * 0.0015);
  clampCam();
  // Keep the point under the cursor under the cursor. Without this, zooming
  // on a phone-sized viewport walks the thing you are looking at off screen.
  cam.cx += before.x - wx(mx);
  cam.cy += before.y - wy(my);
  clampCam();
  draw();
}, { passive: false });

function onClick(mx, my, button) {
  var w = { x: Math.round(wx(mx)), y: Math.round(wy(my)) };

  if (setPadMode) {
    pad = w; setPadMode = false;
    document.getElementById('padbtn').classList.remove('on');
    draw();
    return;
  }

  var hit = pick(mx, my);

  // A card is armed: this click is the second half of the two-tap sortie.
  if (armed !== null && button !== 2) {
    var body = { kind: 'launch', defId: armed, x: pad.x, y: pad.y, n: V ? V.tick % 7 : 0 };
    if (hit && hit.kind === 'foe') body.target = hit.h;
    else pendingMoves.push(w);
    command(body);
    armed = null;
    drawHangar();
    return;
  }

  // Right-click (or long-press equivalent) with something of yours selected is
  // an order: attack what you clicked, or go where you clicked.
  if (button === 2 && sel && sel.kind === 'own') {
    if (hit && hit.kind === 'foe') command({ kind: 'attack', subject: sel.h, target: hit.h });
    else command({ kind: 'move', subject: sel.h, x: w.x, y: w.y });
    return;
  }

  sel = hit ? { kind: hit.kind, h: hit.h } : null;
  draw();
}

document.getElementById('hangar').addEventListener('click', function (ev) {
  var card = ev.target.closest('.card');
  if (!card) return;
  var def = parseInt(card.getAttribute('data-def'), 10);
  armed = (armed === def) ? null : def;
  drawHangar();
});

document.getElementById('side').addEventListener('click', function (ev) {
  var row = ev.target.closest('.row');
  if (!row || !row.getAttribute('data-h')) return;

  // With a card armed, a contact row is a launch. Two taps and no aiming: an
  // inbound FPV is a six-pixel diamond crossing the screen, and requiring the
  // player to hit it with a pointer made a mechanic out of a dexterity test.
  if (armed !== null && row.getAttribute('data-k') === 'foe') {
    command({ kind: 'launch', defId: armed, x: pad.x, y: pad.y,
              target: parseInt(row.getAttribute('data-h'), 10), n: V ? V.tick % 7 : 0 });
    armed = null;
    drawHangar();
    return;
  }

  sel = { kind: row.getAttribute('data-k'), h: parseInt(row.getAttribute('data-h'), 10) };
  var e = selected();
  if (e) { cam.cx = e.x; cam.cy = e.y; clampCam(); }
  draw();
});

document.getElementById('playbtn').onclick = function () {
  control({ action: V && V.paused ? 'resume' : 'pause' });
};
document.getElementById('stepbtn').onclick = function () { control({ action: 'step' }); };
document.getElementById('resetbtn').onclick = function () {
  control({ action: 'reset' }); events = []; since = 0; knownHandles = {}; sel = null;
};
document.getElementById('padbtn').onclick = function () {
  setPadMode = !setPadMode;
  this.classList.toggle('on', setPadMode);
};
document.getElementById('emitbtn').onclick = function () {
  var e = selected();
  if (!sel || sel.kind !== 'own' || !e || e.emitting === undefined) return;
  command({ kind: 'emitting', subject: e.h, on: e.emitting ? 0 : 1 });
};
document.getElementById('sensbtn').onclick = function () {
  showSensors = !showSensors;
  this.classList.toggle('on', showSensors);
  draw();
};
document.getElementById('sidebtn').onclick = function () {
  document.getElementById('side').classList.toggle('open');
};
['spd1', 'spd2', 'spd4'].forEach(function (id, i) {
  document.getElementById(id).onclick = function () {
    control({ action: 'speed', speed: [1, 2, 4][i] });
    document.querySelectorAll('.spd').forEach(function (b) { b.classList.remove('on'); });
    this.classList.add('on');
  };
});

window.addEventListener('keydown', function (ev) {
  if (ev.code === 'Space') { ev.preventDefault(); document.getElementById('playbtn').click(); }
  else if (ev.key === '.') control({ action: 'step' });
  else if (ev.key === 'Escape') { armed = null; setPadMode = false; drawHangar(); }
  else if (ev.key === 'e' || ev.key === 'E') document.getElementById('emitbtn').click();
  else if (ev.key === '+' || ev.key === '=') { cam.ppm *= 1.3; clampCam(); draw(); }
  else if (ev.key === '-') { cam.ppm /= 1.3; clampCam(); draw(); }
  else if (ev.key >= '1' && ev.key <= '7') {
    var d = S.hangar[parseInt(ev.key, 10) - 1];
    if (d) { armed = (armed === d.defId) ? null : d.defId; drawHangar(); }
  }
});

// ---------------------------------------------------------------------------
// Boot

getJson('/api/static').then(function (s) {
  S = s;
  pad = { x: s.pad.x, y: s.pad.y };
  resize();
  buildTerrain();
  // The scenario's framing, fitted to whatever viewport this is. A phone and a
  // desktop open on the same ground at different zooms, which is the whole
  // argument in docs/SCALE.md's final correction made concrete.
  fitTo(s.view.cx, s.view.cy, s.view.widthMetres);
  poll();
  setInterval(poll, 100);
  (function loop() { draw(); requestAnimationFrame(loop); })();
});
