# Mosaic Image Creator

Introduction

The Mosaic Image Creator is a Python script that builds a mosaic from a collection of images, using a target image as the guide. The mosaic is created by dividing the target image into cells and matching each cell with the closest matching image from the collection. The result is a visually stunning representation of the original image using smaller images.

# Features

Image Preprocessing: Converts, crops, and resizes images to ensure consistency.
Dynamic Matching: Uses color averages to match cells with the closest images.
Randomized Selection: Chooses from top matching images to avoid repetition.
Control of Image Repetition: Limits repetition in rows and columns to enhance visual appeal.
Design Decisions

# Image Preprocessing
Conversion to JPEG: This ensures all images are in a consistent format, making processing easier.
Cropping and Resizing: Standardizing the size of the images ensures that they fit perfectly into the mosaic grid. Aspect ratios differing by more than 50% are cropped, while others are resized, balancing the need to maintain original proportions with the necessity of fitting into a square grid.
Dynamic Matching
3x3 Sub-Cell Averaging: The script calculates average colors for 3x3 sub-cells in each image. This improves matching accuracy by considering color distribution rather than just overall average color.
Distance Function for Matching: The sum of squared differences between color components is used to find the closest matching images, providing an effective similarity measure.
Randomized Selection
Choosing from Top Matches: Selecting randomly from the top 5 matching images increases variety while maintaining visual accuracy. This ensures that the mosaic is not monotonous.
Control of Image Repetition
Limiting Repetition in Rows and Columns: Restricting the same image from being used consecutively in rows and columns adds to the visual diversity of the mosaic. Limitations are configurable based on preferences.
Trade-offs

# Grid and Subgrid Analysis
# Grid Division

The target image is divided into a grid of cells, each of which will be replaced by an image from the collection. The choice of 99x99 pixels for the cell size is a balance between granularity and performance; smaller cells would allow more detail but increase computation time.

# Subgrid Analysis

Each 99x99 cell is further divided into a 3x3 subgrid of 11x11 pixels. This subgrid analysis allows for more nuanced matching:

Averaging Entire Cell: If the entire cell's average color were used for matching, finer details like lines and patterns within the cell would be lost. For example, a cell with a white top half and black bottom half would average to a gray color, and the image selected might not represent the original pattern.
Subgrid Averaging: By averaging the color within each 11x11 sub-cell, the script captures more specific color patterns. In the previous example, the script would recognize the white top half and black bottom half and could select an image with a similar distribution, thus preserving the original pattern.
Implications for Selection of Photos

Color and Pattern Considerations: When selecting images for the mosaic, consider not just the overall color but also the distribution of colors and patterns within the image. Images with distinct patterns and contrasts can lead to more visually engaging mosaics.
Size and Aspect Ratio: While the script can handle various sizes and aspect ratios by cropping and resizing, the original proportions of images may be altered. Selecting images that are closer to a square aspect ratio or that can be cropped without losing key features will provide the best results.
Examples

Line Preservation: If a cell in the target image contains a horizontal line, the subgrid analysis can identify this pattern and select an image with a similar line, rather than averaging to a uniform color.
Texture Matching: If a cell has a textured pattern, like a gradient or speckled effect, the subgrid analysis helps in selecting an image that matches this texture, creating a more visually coherent mosaic.

Processing Time: The preprocessing and matching algorithms can be computationally intensive, especially with large image collections. The chosen methods balance accuracy and visual appeal with computational efficiency.
Quality vs Diversity: While restricting image repetition increases diversity, it may sometimes lead to suboptimal matches. The implemented logic carefully balances these aspects.
Usage

# Download or clone the repository.
Modify the script to include the paths to your target image and the directory containing the mosaic images, or input them when prompted.
Run the script, and the mosaic will be saved as 'fnm-thesand.jpg' in the current directory.
Requirements

Python 3.x
PIL (Python Imaging Library)

The Mosaic Image Creator is a powerful and flexible tool for creating image mosaics. By leveraging intelligent preprocessing, matching, and post-processing techniques, it creates visually appealing mosaics that can be customized to suit various artistic needs.
