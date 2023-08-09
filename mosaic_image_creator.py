import os
import tempfile
import random
from PIL import Image
from collections import Counter, deque

# Function to convert images to JPEG format and crop/resize them to standardize their size.
def convert_to_jpg_and_crop(img):
    if img.mode != 'RGB':
        img = img.convert('RGB')
    width, height = img.size
    aspect_ratio_difference = abs(width - height) / max(width, height)
    if aspect_ratio_difference > 0.5:  # If the difference is more than 50%, crop the image.
        min_side = min(img.size)
        max_side = max(img.size)
        left = (max_side - min_side) / 2
        top = (max_side - min_side) / 2
        right = (max_side + min_side) / 2
        bottom = (max_side + min_side) / 2
        img = img.crop((left, top, right, bottom))
    else:  # If the difference is less than or equal to 50%, resize the image.
        min_dim = min(width, height)
        img = img.resize((min_dim, min_dim))
    img = img.resize((99, 99), Image.LANCZOS)
    return img

# Change the following path to the path of your target image
target_image_path = '<Path_to_your_target_image>'
target_image = Image.open(target_image_path)

mosaic_images = []

# Creating a temporary directory to store intermediate images
temp_dir = tempfile.mkdtemp()

# Change the following path to the directory containing your mosaic images
mosaic_images_path = '<Path_to_your_mosaic_images_directory>'

# Process each image in the directory, standardizing their size
for filename in os.listdir(mosaic_images_path):
    if filename.endswith(('.jpg', '.jpeg', '.png', '.gif')):
        img = Image.open(os.path.join(mosaic_images_path, filename))
        img = convert_to_jpg_and_crop(img)
        img.thumbnail((99, 99))  # Reduce image size
        temp_filename = os.path.join(temp_dir, filename)
        img.save(temp_filename)  # Save the reduced image to temp folder
        mosaic_images.append(temp_filename)

# Resize the target image to fit the grid
target_image = target_image.resize((target_image.width // 99 * 99, target_image.height // 99 * 99))

# Divide the target image into 99x99 cells
grid_cells = [target_image.crop((i, j, i + 99, j + 99)) for i in range(0, target_image.width, 99) for j in range(0, target_image.height, 99)]

# calculate the average colors of the 3x3 sub-cells for each image
avg_colors_mosaic_images = []
for img_path in mosaic_images:
    img = Image.open(img_path)
    sub_cells_img = [img.crop((x, y, x + 11, y + 11)) for x in range(0, 99, 11) for y in range(0, 99, 11)]
    avg_colors_mosaic_images.append([sub.resize((1, 1)).getpixel((0, 0)) for sub in sub_cells_img])

final_images = []
image_counts = Counter()
last_images_row = deque(maxlen=3)  # control how many images can't be the same in a row
last_images_columns = [deque(maxlen=1) for _ in range(target_image.width // 99)]  # control how many images can't be the same in a column

for i, cell in enumerate(grid_cells):
    print(f"Generating mosaic {i+1}/{len(grid_cells)} ({(i+1)*100/len(grid_cells):.1f}%)", end='\r')
    sub_cells_target = [cell.crop((x, y, x + 11, y + 11)) for x in range(0, 99, 11) for y in range(0, 99, 11)]
    avg_colors_target = [sub.resize((1, 1)).getpixel((0, 0)) for sub in sub_cells_target]
    
    def diff_func(avg_colors_img):
        return sum(sum((p1 - p2) ** 2 for p1, p2 in zip(avg_target, avg_img)) for avg_target, avg_img in zip(avg_colors_target, avg_colors_img))

    closest_imgs_indices = sorted(range(len(mosaic_images)), key=lambda index: diff_func(avg_colors_mosaic_images[index]))
    column_index = (i // (target_image.width // 99)) % (target_image.width // 99)
    closest_imgs_indices = [index for index in closest_imgs_indices if mosaic_images[index] not in last_images_row and mosaic_images[index] not in last_images_columns[column_index] and image_counts[mosaic_images[index]] < 16]
    closest_imgs_indices = closest_imgs_indices[:5]
    if closest_imgs_indices:
        selected_img_index = random.choice(closest_imgs_indices)
        selected_img_path = mosaic_images[selected_img_index]
        image_counts[selected_img_path] += 1
        last_images_row.append(selected_img_path)
        last_images_columns[column_index].append(selected_img_path)
        selected_img = Image.open(selected_img_path).resize((99, 99))
        final_images.append(selected_img)
    else:
        print(f"Warning: No suitable image found for cell {i+1}. Skipping this cell.")

mosaic = Image.new('RGB', target_image.size)
for img, (i, j) in zip(final_images, [(i, j) for i in range(0, target_image.width, 99) for j in range(0, target_image.height, 99)]):
    mosaic.paste(img, (i, j))

# Save the final mosaic image
output_image_path = 'REPLACE_WITH_YOUR_FILENAME.JPG'
mosaic.save(output_image_path)
print(f"\nMosaic creation complete! Saved to {output_image_path}")

# calculate the average colors of the 9x9 sub-cells for each image
avg_colors_mosaic_images = []
for img_path in mosaic_images:
    img = Image.open(img_path)
    sub_cells_img = [img.crop((x, y, x + 11, y + 11)) for x in range(0, 99, 11) for y in range(0, 99, 11)]
    avg_colors_mosaic_images.append([sub.resize((1, 1)).getpixel((0, 0)) for sub in sub_cells_img])



