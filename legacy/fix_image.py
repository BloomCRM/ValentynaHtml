#!/usr/bin/env python3
"""
Script to create a fresh copy of main.jpg with proper encoding
"""

from PIL import Image
import os

def fix_image():
    try:
        # Open the original image
        with Image.open('img/main.jpg') as img:
            print(f"Original image: {img.format}, {img.size}, {img.mode}")
            
            # Convert to RGB if needed
            if img.mode != 'RGB':
                img = img.convert('RGB')
                print("Converted to RGB mode")
            
            # Create a fresh copy with explicit settings
            output_path = 'img/main_fixed.jpg'
            img.save(output_path, 'JPEG', quality=90, optimize=True, progressive=True)
            
            # Verify the new file
            with Image.open(output_path) as test_img:
                print(f"New image: {test_img.format}, {test_img.size}, {test_img.mode}")
                test_img.load()
                print("New image loads successfully")
            
            # Get file sizes
            original_size = os.path.getsize('img/main.jpg')
            new_size = os.path.getsize(output_path)
            
            print(f"Original size: {original_size/1024/1024:.2f}MB")
            print(f"New size: {new_size/1024/1024:.2f}MB")
            
            # Replace the original with the fixed version
            os.remove('img/main.jpg')
            os.rename(output_path, 'img/main.jpg')
            
            print("✓ Image fixed and replaced successfully!")
            return True
            
    except Exception as e:
        print(f"Error: {e}")
        return False

if __name__ == "__main__":
    fix_image()
