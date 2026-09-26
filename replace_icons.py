import os

def replace_in_file(filepath, replacements):
    with open(filepath, 'r') as f:
        content = f.read()
    for old, new in replacements:
        content = content.replace(old, new)
    with open(filepath, 'w') as f:
        f.write(content)

# LoginPage
replace_in_file('Views/LoginPage.xaml', [
    ('Text="🚚"', 'Text="&#xe558;" FontFamily="MaterialIcons" TextColor="White"'),
])

# DashboardPage
replace_in_file('Views/DashboardPage.xaml', [
    ('Text="👤"', 'Text="&#xe7fd;" FontFamily="MaterialIcons" TextColor="#4B5563"'),
    ('Text="📦"', 'Text="&#xe1a1;" FontFamily="MaterialIcons" TextColor="#4B5563"'),
    ('Text="📍"', 'Text="&#xe0c8;" FontFamily="MaterialIcons" TextColor="#4B5563"'),
])

# DeliveryPage
replace_in_file('Views/DeliveryPage.xaml', [
    ('Text="👤"', 'Text="&#xe7fd;" FontFamily="MaterialIcons" TextColor="#4B5563"'),
    ('Text="📦"', 'Text="&#xe1a1;" FontFamily="MaterialIcons" TextColor="#4B5563"'),
    ('Text="📍"', 'Text="&#xe0c8;" FontFamily="MaterialIcons" TextColor="#4B5563"'),
    ('Text="📷"', 'Text="&#xe412;" FontFamily="MaterialIcons" TextColor="#9CA3AF"'),
    ('Text="📷 Capture Photo"', 'Text="Capture Photo" ImageSource="{FontImageSource FontFamily=MaterialIcons, Glyph=&#xe412;, Color=White, Size=20}"'),
    ('Text="✅ Complete Delivery"', 'Text="Complete Delivery" ImageSource="{FontImageSource FontFamily=MaterialIcons, Glyph=&#xe86c;, Color=White, Size=20}"'),
])
