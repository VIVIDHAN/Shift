import os

file_path = "index.html"
with open(file_path, "r") as f:
    content = f.read()

# Add IDs to the stats cards
content = content.replace('<h3 class="text-3xl font-black text-gray-900 mt-2">124</h3>', '<h3 id="statTotal" class="text-3xl font-black text-gray-900 mt-2">0</h3>')
content = content.replace('<h3 class="text-3xl font-black text-gray-900 mt-2">98</h3>', '<h3 id="statDelivered" class="text-3xl font-black text-gray-900 mt-2">0</h3>')
content = content.replace('<h3 class="text-3xl font-black text-gray-900 mt-2">26</h3>', '<h3 id="statPending" class="text-3xl font-black text-gray-900 mt-2">0</h3>')

# Update loadData to calculate stats
old_js = "document.getElementById('ordersTableBody').innerHTML = html;"
new_js = """document.getElementById('ordersTableBody').innerHTML = html;
                    
                    // Update stats
                    let total = data.orders ? data.orders.length : 0;
                    let delivered = data.orders ? data.orders.filter(o => o.status === 'Delivered').length : 0;
                    let pending = total - delivered;
                    
                    document.getElementById('statTotal').innerText = total;
                    document.getElementById('statDelivered').innerText = delivered;
                    document.getElementById('statPending').innerText = pending;"""

content = content.replace(old_js, new_js)

with open(file_path, "w") as f:
    f.write(content)
