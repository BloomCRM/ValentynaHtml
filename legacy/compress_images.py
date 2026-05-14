#!/usr/bin/env python3
"""
Script to compress JPG images in the img directory
"""

import os
import sys
from PIL import Image
import glob

def compress_image(input_path, output_path, quality=85, max_width=None, max_height=None):
    """
    Compress a JPG image with specified quality and optional resizing
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
            
            # Save with compression
            img.save(output_path, 'JPEG', quality=quality, optimize=True)
            
            # Get file sizes
            original_size = os.path.getsize(input_path)
            compressed_size = os.path.getsize(output_path)
            compression_ratio = (1 - compressed_size / original_size) * 100
            
            print(f"✓ {os.path.basename(input_path)}: {original_size/1024/1024:.1f}MB → {compressed_size/1024/1024:.1f}MB ({compression_ratio:.1f}% reduction)")
            
            return True
            
    except Exception as e:
        print(f"✗ Error processing {input_path}: {e}")
        return False

def main():
    # Find all JPG files in the img directory
    jpg_files = glob.glob("img/*.jpg") + glob.glob("img/*.JPG")
    
    if not jpg_files:
        print("No JPG files found in the img directory")
        return
    
    print(f"Found {len(jpg_files)} JPG files to compress:")
    for file in jpg_files:
        print(f"  - {file}")
    
    print("\nStarting compression...")
    
    # Create backup directory
    backup_dir = "img/backup"
    os.makedirs(backup_dir, exist_ok=True)
    
    successful = 0
    total_original_size = 0
    total_compressed_size = 0
    
    for jpg_file in jpg_files:
        # Create backup
        backup_path = os.path.join(backup_dir, os.path.basename(jpg_file))
        if not os.path.exists(backup_path):
            import shutil
            shutil.copy2(jpg_file, backup_path)
        
        # Compress the image
        if compress_image(jpg_file, jpg_file, quality=85):
            successful += 1
            total_original_size += os.path.getsize(backup_path)
            total_compressed_size += os.path.getsize(jpg_file)
    
    print(f"\nCompression complete!")
    print(f"Successfully compressed {successful}/{len(jpg_files)} images")
    print(f"Total size reduction: {total_original_size/1024/1024:.1f}MB → {total_compressed_size/1024/1024:.1f}MB")
    print(f"Overall compression: {(1 - total_compressed_size/total_original_size)*100:.1f}%")
    print(f"Backups saved in: {backup_dir}")

if __name__ == "__main__":
    main()
