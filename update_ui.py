import os
import re

file_path = "index.html"
with open(file_path, "r") as f:
    content = f.read()

# Update head with Google Fonts and Icons
head_old = """    <title>Shunmugarai Admin</title>
    <script src="https://cdn.tailwindcss.com"></script>
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">"""

head_new = """    <title>Shunmugarai Admin</title>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">
    <link href="https://fonts.googleapis.com/icon?family=Material+Icons+Round" rel="stylesheet">
    <script src="https://cdn.tailwindcss.com"></script>
    <style>
        body { font-family: 'Inter', sans-serif; }
        .material-icons-round { vertical-align: middle; font-size: inherit; }
    </style>"""

content = content.replace(head_old, head_new)

# Replace FontAwesome icons with Google Material Icons
icon_map = {
    "fa-truck-fast": "local_shipping",
    "fa-chart-pie": "pie_chart",
    "fa-box": "inventory_2",
    "fa-users": "people",
    "fa-plus": "add",
    "fa-boxes-stacked": "inventory",
    "fa-circle-check": "check_circle",
    "fa-clock-rotate-left": "history",
    "fa-eye": "visibility",
    "fa-trash": "delete",
    "fa-xmark": "close",
    "fa-user-plus": "person_add"
}

for fa, mi in icon_map.items():
    content = re.sub(rf'<i class="fa-solid {fa}[^"]*"></i>', f'<span class="material-icons-round">{mi}</span>', content)

with open(file_path, "w") as f:
    f.write(content)
