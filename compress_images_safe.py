#!/usr/bin/env python3
"""
Safe script to compress JPG images in the img directory
Creates compressed versions without overwriting originals
"""

import os
import sys
from PIL import Image
import glob
import shutil

def compress_image_safe(input_path, output_path, quality=85, max_width=None, max_height=None):
    """
    Safely compress a JPG image with specified quality and optional resizing
    """
    try:
        # Open the image
        with Image.open(input_path) as img:
            # Convert to RGB if necessary (in case of RGBA or other modes)
            if img.mode != 'RGB':
                img = img.convert('RGB')
            
            # Resize if max dimensions are specified
            if max_width or max_height:
                width, height = img.size
                
                if max_width and width > max_width:
                    ratio = max_width / width
                    new_width = max_width
                    new_height = int(height * ratio)
                elif max_height and height > max_height:
                    ratio = max_height / height
                    new_height = max_height
                    new_width = int(width * ratio)
                else:
                    new_width, new_height = width, height
                
                img = img.resize((new_width, new_height), Image.Resampling.LANCZOS)
            
            # Save with compression to a temporary file first
            temp_path = output_path + ".tmp"
            img.save(temp_path, 'JPEG', quality=quality, optimize=True)
            
            # Verify the file was created successfully
            if os.path.exists(temp_path) and os.path.getsize(temp_path) > 0:
                # Test if the file can be opened
                with Image.open(temp_path) as test_img:
                    test_img.verify()
                
                # If verification passes, move to final location
                shutil.move(temp_path, output_path)
                
                # Get file sizes
                original_size = os.path.getsize(input_path)
                compressed_size = os.path.getsize(output_path)
                compression_ratio = (1 - compressed_size / original_size) * 100
                
                print(f"✓ {os.path.basename(input_path)}: {original_size/1024/1024:.1f}MB → {compressed_size/1024/1024:.1f}MB ({compression_ratio:.1f}% reduction)")
                
                return True
            else:
                print(f"✗ Failed to create compressed file for {input_path}")
                if os.path.exists(temp_path):
                    os.remove(temp_path)
                return False
            
    except Exception as e:
        print(f"✗ Error processing {input_path}: {e}")
        # Clean up temp file if it exists
        temp_path = output_path + ".tmp"
        if os.path.exists(temp_path):
            os.remove(temp_path)
        return False

def main():
    # Find all JPG files in the img directory
    jpg_files = glob.glob("img/*.jpg") + glob.glob("img/*.JPG")
    
    if not jpg_files:
        print("No JPG files found in the img directory")
        return
    
    # Remove duplicates (case sensitivity)
    unique_files = list(set([f.lower() for f in jpg_files]))
    jpg_files = [f for f in jpg_files if f.lower() in unique_files]
    
    print(f"Found {len(jpg_files)} JPG files to compress:")
    for file in jpg_files:
        print(f"  - {file}")
    
    print("\nStarting safe compression...")
    
    # Create compressed directory
    compressed_dir = "img/compressed"
    os.makedirs(compressed_dir, exist_ok=True)
    
    successful = 0
    total_original_size = 0
    total_compressed_size = 0
    
    for jpg_file in jpg_files:
        filename = os.path.basename(jpg_file)
        compressed_path = os.path.join(compressed_dir, filename)
        
        # Compress the image to the compressed directory
        if compress_image_safe(jpg_file, compressed_path, quality=85):
            successful += 1
            total_original_size += os.path.getsize(jpg_file)
            total_compressed_size += os.path.getsize(compressed_path)
    
    print(f"\nSafe compression complete!")
    print(f"Successfully compressed {successful}/{len(jpg_files)} images")
    print(f"Total size reduction: {total_original_size/1024/1024:.1f}MB → {total_compressed_size/1024/1024:.1f}MB")
    print(f"Overall compression: {(1 - total_compressed_size/total_original_size)*100:.1f}%")
    print(f"Compressed files saved in: {compressed_dir}")
    print(f"\nTo replace originals with compressed versions, run:")
    print(f"Get-ChildItem '{compressed_dir}' | ForEach-Object {{ Copy-Item $_.FullName 'img/' -Force }}")

if __name__ == "__main__":
    main()
