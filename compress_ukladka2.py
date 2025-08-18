#!/usr/bin/env python3
"""
Script to compress ukladka2.JPG with aggressive compression
"""

from PIL import Image
import os

def compress_ukladka2():
    try:
        # Open the original image
        with Image.open('img/ukladka2.JPG') as img:
            print(f"Original image: {img.format}, {img.size}, {img.mode}")
            original_size = os.path.getsize('img/ukladka2.JPG')
            print(f"Original size: {original_size/1024/1024:.2f}MB")
            
            # Convert to RGB if needed
            if img.mode != 'RGB':
                img = img.convert('RGB')
            
            # Resize if too large (max width 1920px)
            width, height = img.size
            if width > 1920:
                ratio = 1920 / width
                new_width = 1920
                new_height = int(height * ratio)
                img = img.resize((new_width, new_height), Image.Resampling.LANCZOS)
                print(f"Resized to: {new_width}x{new_height}")
            
            # Create compressed version with lower quality
            output_path = 'img/ukladka2_compressed.jpg'
            img.save(output_path, 'JPEG', quality=70, optimize=True, progressive=True)
            
            # Verify the new file
            with Image.open(output_path) as test_img:
                print(f"New image: {test_img.format}, {test_img.size}, {test_img.mode}")
                test_img.load()
                print("New image loads successfully")
            
            new_size = os.path.getsize(output_path)
            compression_ratio = (1 - new_size / original_size) * 100
            
            print(f"New size: {new_size/1024/1024:.2f}MB")
            print(f"Compression: {compression_ratio:.1f}%")
            
            # Replace the original with the compressed version
            os.remove('img/ukladka2.JPG')
            os.rename(output_path, 'img/ukladka2.JPG')
            
            print("✓ ukladka2.JPG compressed and replaced successfully!")
            return True
            
    except Exception as e:
        print(f"Error: {e}")
        return False

if __name__ == "__main__":
    compress_ukladka2()
