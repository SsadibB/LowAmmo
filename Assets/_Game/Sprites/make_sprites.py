import zlib
import struct
import os

def make_png(width, height, get_pixel_func, filepath):
    # Raw RGBA data
    raw_data = bytearray()
    for y in range(height):
        raw_data.append(0)  # filter type 0 (None)
        for x in range(width):
            r, g, b, a = get_pixel_func(x, y, width, height)
            raw_data.extend([r, g, b, a])
    
    # Compress with zlib
    compressed = zlib.compress(raw_data)
    
    def chunk(tag, data):
        length = struct.pack('>I', len(data))
        crc = struct.pack('>I', zlib.crc32(tag + data) & 0xffffffff)
        return length + tag + data + crc

    png = bytearray(b'\x89PNG\r\n\x1a\n')
    # IHDR
    ihdr_data = struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0)
    png.extend(chunk(b'IHDR', ihdr_data))
    # IDAT
    png.extend(chunk(b'IDAT', compressed))
    # IEND
    png.extend(chunk(b'IEND', b''))
    
    os.makedirs(os.path.dirname(filepath), exist_ok=True)
    with open(filepath, 'wb') as f:
        f.write(png)

# 1. Ground: 32x32 dark steel/grey with light bevel
def ground_pix(x, y, w, h):
    if x == 0 or y == 0: return (100, 110, 125, 255)
    if x == w-1 or y == h-1: return (40, 45, 55, 255)
    if (x % 8 == 0) or (y % 8 == 0): return (55, 60, 70, 255)
    return (70, 78, 90, 255)

# 2. Player: 32x48 cyan hero with visor
def player_pix(x, y, w, h):
    if x < 4 or x >= w - 4 or y < 4 or y >= h - 4:
        return (0, 0, 0, 0)
    # Visor
    if 8 <= y <= 16 and 14 <= x <= 26:
        return (255, 230, 80, 255)
    # Border
    if x == 4 or x == w - 5 or y == 4 or y == h - 5:
        return (20, 60, 100, 255)
    return (40, 150, 230, 255)

# 3. Gun: 24x12 sleek black/grey pistol
def gun_pix(x, y, w, h):
    # Barrel
    if 3 <= y <= 7 and 0 <= x <= 20:
        return (200, 205, 215, 255)
    # Grip
    if 7 <= y <= 11 and 4 <= x <= 10:
        return (70, 75, 85, 255)
    return (0, 0, 0, 0)

# 4. Crate: 32x32 wooden crate with cross braces
def crate_pix(x, y, w, h):
    if x == 0 or x == w-1 or y == 0 or y == h-1:
        return (120, 75, 30, 255)
    if x < 3 or x >= w-3 or y < 3 or y >= h-3:
        return (160, 105, 50, 255)
    # Diagonals
    if abs(x - y) <= 1 or abs((w - 1 - x) - y) <= 1:
        return (140, 90, 40, 255)
    return (195, 135, 65, 255)

# 5. Rope: 8x32 coiled hemp rope
def rope_pix(x, y, w, h):
    if x < 2 or x >= 6: return (0, 0, 0, 0)
    if (x + y) % 3 == 0:
        return (160, 130, 80, 255)
    return (200, 170, 110, 255)

# 6. Pressure Plate: 48x16 industrial plate
def plate_pix(x, y, w, h):
    if y < 6: return (0, 0, 0, 0)
    if y >= 12: # Base
        return (80, 85, 95, 255)
    # Button top
    if 4 <= x <= w - 5:
        return (230, 70, 70, 255)
    return (0, 0, 0, 0)

# 7. Door: 24x64 metal blast door
def door_pix(x, y, w, h):
    if x == 0 or x == w-1 or y == 0 or y == h-1:
        return (40, 50, 60, 255)
    # Hazard stripes
    if (x + y) % 12 < 4:
        return (220, 180, 40, 255)
    return (90, 100, 115, 255)

# 8. Fire: 32x32 flame
def fire_pix(x, y, w, h):
    cx, cy = w / 2, h / 2
    dx, dy = (x - cx), (y - cy)
    dist = (dx*dx*1.4 + dy*dy)
    if dist > 180: return (0, 0, 0, 0)
    if dist < 40: return (255, 255, 200, 255) # Yellow center
    if dist < 90: return (255, 140, 20, 255) # Orange
    return (220, 40, 20, 220) # Red edge

# 9. Water: 32x32 flowing blue stream
def water_pix(x, y, w, h):
    if (x + y * 2) % 10 < 3:
        return (180, 230, 255, 220)
    return (40, 140, 240, 190)

# 10. Valve: 32x32 round wheel
def valve_pix(x, y, w, h):
    cx, cy = w / 2 - 0.5, h / 2 - 0.5
    r2 = (x - cx)**2 + (y - cy)**2
    if 100 <= r2 <= 196: return (210, 50, 50, 255) # Rim
    if r2 <= 16: return (180, 40, 40, 255) # Center hub
    if (abs(x - cx) <= 1.5 or abs(y - cy) <= 1.5) and r2 < 144: return (230, 70, 70, 255) # Spokes
    return (0, 0, 0, 0)

# 11. Platform: 64x16 steel girder
def platform_pix(x, y, w, h):
    if y == 0 or y == h-1 or x == 0 or x == w-1: return (60, 65, 75, 255)
    if (x % 16 < 2): return (100, 110, 125, 255)
    return (130, 140, 155, 255)

# 12. Exit: 32x48 glowing portal
def exit_pix(x, y, w, h):
    if x == 0 or x == w-1 or y == 0: return (80, 90, 100, 255)
    cx, cy = w / 2 - 0.5, h / 2
    if abs(x - cx) < 12 and y > 6:
        return (50, 220, 130, 220) # Green energy field
    return (110, 120, 130, 255)

# 13. Glass: 16x48 translucent reinforced glass
def glass_pix(x, y, w, h):
    if x == 0 or x == w-1 or y == 0 or y == h-1: return (120, 190, 220, 200)
    if (x + y) % 16 == 0: return (220, 245, 255, 180) # Glint
    return (90, 170, 210, 90)

# 14. Switch: 24x32 electrical switch
def switch_pix(x, y, w, h):
    if 6 <= x <= 18 and 6 <= y <= 26:
        return (180, 185, 195, 255)
    if 10 <= x <= 14 and y < 14:
        return (230, 50, 50, 255)
    return (50, 55, 65, 255)

out_dir = r"c:\Users\Sadib\Documents\GitHub\LowAmmo\Assets\_Game\Sprites"
make_png(32, 32, ground_pix, os.path.join(out_dir, "ground.png"))
make_png(32, 48, player_pix, os.path.join(out_dir, "player.png"))
make_png(24, 12, gun_pix, os.path.join(out_dir, "gun.png"))
make_png(32, 32, crate_pix, os.path.join(out_dir, "crate.png"))
make_png(8, 32, rope_pix, os.path.join(out_dir, "rope.png"))
make_png(48, 16, plate_pix, os.path.join(out_dir, "plate.png"))
make_png(24, 64, door_pix, os.path.join(out_dir, "door.png"))
make_png(32, 32, fire_pix, os.path.join(out_dir, "fire.png"))
make_png(32, 32, water_pix, os.path.join(out_dir, "water.png"))
make_png(32, 32, valve_pix, os.path.join(out_dir, "valve.png"))
make_png(64, 16, platform_pix, os.path.join(out_dir, "platform.png"))
make_png(32, 48, exit_pix, os.path.join(out_dir, "exit.png"))
make_png(16, 48, glass_pix, os.path.join(out_dir, "glass.png"))
make_png(24, 32, switch_pix, os.path.join(out_dir, "switch.png"))
print("All 14 sprites generated successfully!")
