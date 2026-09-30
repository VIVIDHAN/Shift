        // Simple Auth Check
        if (sessionStorage.getItem('adminLoggedIn') !== 'true') {
            window.location.href = '/index.html';
        }
        tailwind.config = {
            theme: {
                extend: {
                    colors: {
                        primary: '#1E3A8A', // Deep Blue
                        success: '#10B981', // Emerald Green
                        accent: '#3B82F6',  // Sky Blue
                        surface: '#FFFFFF',
                        background: '#F3F4F6'
                    },
                    fontFamily: {
                        sans: ['Inter', 'sans-serif'],
                    }
                }
            }
        }
        function unlockSettings() {
            document.getElementById('settingShopName').disabled = false;
            document.getElementById('settingShopPhone').disabled = false;
            document.getElementById('settingShopAddress').disabled = false;
            document.getElementById('settingSmsTemplate').disabled = false;
            
            document.getElementById('btnUnlockSettings').style.display = 'none';
            document.getElementById('btnSaveSettings').style.display = 'block';
        }

        function lockSettings() {
            document.getElementById('settingShopName').disabled = true;
            document.getElementById('settingShopPhone').disabled = true;
            document.getElementById('settingShopAddress').disabled = true;
            document.getElementById('settingSmsTemplate').disabled = true;
            
            document.getElementById('btnUnlockSettings').style.display = 'block';
            document.getElementById('btnSaveSettings').style.display = 'none';
        }

        function showTab(tabName) {
            document.querySelectorAll('.erp-tab').forEach(t => {
                t.style.display = 'none';
                t.classList.add('hidden');
            });
            document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
            
            const tab = document.getElementById('tab-' + tabName);
            tab.style.display = 'block';
            tab.classList.remove('hidden');
            
            const links = document.querySelectorAll('.nav-link');
            links.forEach(l => {
                if (l.getAttribute('onclick') && l.getAttribute('onclick').includes(tabName)) {
                    l.classList.add('active');
                }
            });

            if (tabName === 'settings') {
                loadSettings();
            } else {
                loadDatabase();
            }
        }

        function uiAlert(message) {
            document.getElementById('uiAlertMessage').textContent = message;
            document.getElementById('uiAlertModal').classList.remove('hidden');
        }

        function uiConfirm(message) {
            return new Promise((resolve) => {
                document.getElementById('uiConfirmMessage').textContent = message;
                const modal = document.getElementById('uiConfirmModal');
                modal.classList.remove('hidden');
                
                const okBtn = document.getElementById('uiConfirmOkBtn');
                const cancelBtn = document.getElementById('uiConfirmCancelBtn');
                
                const cleanup = () => {
                    modal.classList.add('hidden');
                    okBtn.removeEventListener('click', onOk);
                    cancelBtn.removeEventListener('click', onCancel);
                };
                
                const onOk = () => { cleanup(); resolve(true); };
                const onCancel = () => { cleanup(); resolve(false); };
                
                okBtn.addEventListener('click', onOk);
                cancelBtn.addEventListener('click', onCancel);
            });
        }

        function uiEditRecord(fields) {
            return new Promise((resolve) => {
                const container = document.getElementById('uiEditFormContainer');
                container.innerHTML = '';
                
                // Build form fields
                fields.forEach(f => {
                    const div = document.createElement('div');
                    let inputHtml = '';
                    if (f.key === 'IsActive') {
                        const isTrue = String(f.value).toLowerCase() === 'true' || f.value === 1 || f.value === '1';
                        inputHtml = `
                            <select id="editField_${f.key}" class="w-full border-gray-300 rounded-md shadow-sm focus:border-blue-500 focus:ring focus:ring-blue-200 focus:ring-opacity-50 text-gray-700 p-2 border">
                                <option value="1" ${isTrue ? 'selected' : ''}>Active (Yes)</option>
                                <option value="0" ${!isTrue ? 'selected' : ''}>Inactive (No)</option>
                            </select>
                        `;
                    } else {
                        inputHtml = `<input type="text" id="editField_${f.key}" value="${f.value}" class="w-full border-gray-300 rounded-md shadow-sm focus:border-blue-500 focus:ring focus:ring-blue-200 focus:ring-opacity-50 text-gray-700 p-2 border" />`;
                    }
                    div.innerHTML = `
                        <label class="block text-sm font-medium text-gray-700 mb-1">${f.key}</label>
                        ${inputHtml}
                    `;
                    container.appendChild(div);
                });
                
                const modal = document.getElementById('uiEditModal');
                modal.classList.remove('hidden');
                
                const okBtn = document.getElementById('uiEditOkBtn');
                const cancelBtn = document.getElementById('uiEditCancelBtn');
                
                const cleanup = () => {
                    modal.classList.add('hidden');
                    okBtn.removeEventListener('click', onOk);
                    cancelBtn.removeEventListener('click', onCancel);
                };
                
                const onOk = () => {
                    cleanup();
                    const results = {};
                    fields.forEach(f => {
                        results[f.key] = document.getElementById(`editField_${f.key}`).value;
                    });
                    resolve(results);
                };
                
                const onCancel = () => { cleanup(); resolve(null); };
                
                okBtn.addEventListener('click', onOk);
                cancelBtn.addEventListener('click', onCancel);
            });
        }

        function logoutAdmin() {
            document.getElementById('logoutModal').classList.remove('hidden');
        }

        function hideLogoutModal() {
            document.getElementById('logoutModal').classList.add('hidden');
        }

        function confirmLogout() {
            sessionStorage.removeItem('adminLoggedIn');
            window.location.href = '/index.html';
        }

        async function runSql(sqlQuery) {
            const res = await fetch('/api/admin/query', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Sql: sqlQuery })
            });
            return await res.json();
        }

        async function deleteRow(tableName, primaryKeyCol, primaryKeyValue) {
            const confirmed = await uiConfirm(`Are you sure you want to delete ${primaryKeyValue} from ${tableName}?`);
            if(!confirmed) return;
            const sql = `DELETE FROM ${tableName} WHERE ${primaryKeyCol} = '${primaryKeyValue}'`;
            const result = await runSql(sql);
            if(result.success) {
                loadDatabase();
            } else {
                uiAlert('Failed to delete: ' + result.error);
            }
        }

        async function editRow(tableName, primaryKeyCol, rowEncoded) {
            const row = JSON.parse(decodeURIComponent(rowEncoded));
            
            // Prepare fields for the multi-field editor
            let editableFields = [];
            for (let key in row) {
                if (key === primaryKeyCol || key === 'CreatedAt' || key === 'PasswordHash' || key === 'OrderDate') continue;
                editableFields.push({ key: key, value: row[key] !== null ? row[key] : '' });
            }
            
            if (editableFields.length === 0) return;
            
            // Show multi-field modal
            const newValues = await uiEditRecord(editableFields);
            if (!newValues) return; // User cancelled
            
            let updates = [];
            for (let key in newValues) {
                let val = newValues[key];
                if (val.toLowerCase() === 'true') val = '1';
                else if (val.toLowerCase() === 'false') val = '0';
                updates.push(`${key} = '${val.replace(/'/g, "''")}'`);
            }
            
            if (updates.length === 0) return;
            
            const sql = `UPDATE ${tableName} SET ${updates.join(', ')} WHERE ${primaryKeyCol} = '${row[primaryKeyCol]}'`;
            const result = await runSql(sql);
            if (result.success) {
                loadDatabase();
            } else {
                uiAlert('Failed to modify: ' + result.error);
            }
        }

        function viewOrderDetails(rowEncoded) {
            const row = JSON.parse(decodeURIComponent(rowEncoded));
            document.getElementById('viewOrderTitle').textContent = `Order ${row.OrderId}`;
            document.getElementById('viewOrderSubtitle').textContent = new Date(row.OrderDate).toLocaleString([], {month:'short', day:'numeric', hour:'2-digit', minute:'2-digit'});
            document.getElementById('viewOrderCustomer').textContent = row.CustomerName;
            document.getElementById('viewOrderPhone').textContent = row.CustomerPhone;
            document.getElementById('viewOrderDriver').textContent = row.DriverPhone || 'Unassigned';
            document.getElementById('viewOrderLocation').textContent = row.Location;
            document.getElementById('viewOrderItems').textContent = row.Items;
            
            let statusHtml = row.Status;
            if (row.Status === 'Delivered') statusHtml = '<span class="badge-success"><i class="fas fa-check-circle mr-1"></i>Delivered</span>';
            else if (row.Status === 'Assigned') statusHtml = '<span class="badge-warning"><i class="fas fa-clock mr-1"></i>Assigned</span>';
            document.getElementById('viewOrderStatus').innerHTML = statusHtml;
            
            const photoContainer = document.getElementById('viewOrderPhotoContainer');
            if (row.DeliveryPhotoUrl) {
                document.getElementById('viewOrderPhoto').src = row.DeliveryPhotoUrl;
                document.getElementById('viewOrderPhotoLink').href = row.DeliveryPhotoUrl;
                photoContainer.classList.remove('hidden');
            } else {
                photoContainer.classList.add('hidden');
            }
            
            // Build Actions
            const actionsContainer = document.getElementById('viewOrderActions');
            actionsContainer.innerHTML = `
                <button onclick="document.getElementById('uiViewOrderModal').classList.add('hidden'); editRow('Orders', 'OrderId', '${rowEncoded}')" class="px-4 py-2 bg-white border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 flex items-center">
                    <i class="fas fa-edit mr-2"></i> Edit Order
                </button>
                <button onclick="document.getElementById('uiViewOrderModal').classList.add('hidden'); deleteRow('Orders', 'OrderId', '${row.OrderId}')" class="px-4 py-2 bg-red-600 border border-transparent rounded-md shadow-sm text-sm font-medium text-white hover:bg-red-700 flex items-center">
                    <i class="fas fa-trash mr-2"></i> Delete
                </button>
            `;
            
            document.getElementById('uiViewOrderModal').classList.remove('hidden');
        }

        function generateTableHtml(tableName, columns, rows, enableDelete = true) {
            if (rows.length === 0) return '<div class="text-center p-6 text-gray-500 bg-gray-50 rounded-lg">No data found.</div>';
            
            const hiddenColumns = ['ShopOwnerPhone', 'PasswordHash'];
            const visibleCols = columns.filter(c => !hiddenColumns.includes(c));

            let html = '<table class="min-w-full"><thead><tr>';
            visibleCols.forEach(c => {
                let headerText = c;
                if (c === 'DeliveryPhotoUrl') headerText = 'Preview';
                html += `<th class="px-6 py-3 border-b border-gray-200 bg-gray-50 text-left text-xs leading-4 font-bold text-gray-500 uppercase tracking-wider">${headerText}</th>`;
            });
            if (enableDelete) html += `<th class="px-6 py-3 border-b border-gray-200 bg-gray-50 text-right text-xs leading-4 font-bold text-gray-500 uppercase tracking-wider">Actions</th>`;
            html += '</tr></thead><tbody>';
            
            const primaryKeyCol = columns[0];

            [...rows].reverse().forEach(row => {
                html += '<tr class="hover:bg-gray-50 transition-colors">';
                visibleCols.forEach(c => {
                    let val = row[c] !== null ? row[c] : '';
                    
                    if (c === 'Status') {
                        if (val === 'Delivered') val = '<span class="badge-success"><i class="fas fa-check-circle mr-1"></i>Delivered</span>';
                        else if (val === 'Assigned') val = '<span class="badge-warning"><i class="fas fa-clock mr-1"></i>Assigned</span>';
                    }
                    else if (c === 'OrderId' || c === 'Id') val = `<span class="font-bold text-gray-900">${val}</span>`;
                    else if (c.includes('Date') || c === 'CreatedAt') {
                        try { val = new Date(val).toLocaleString([], {month:'short', day:'numeric', hour:'2-digit', minute:'2-digit'}); } catch(e){}
                    }
                    else if (c.includes('Phone')) val = `<span class="text-primary font-medium"><i class="fas fa-phone-alt mr-1"></i>${val}</span>`;
                    else if (c === 'DeliveryPhotoUrl' && val) {
                        val = `<a href="${val}" target="_blank"><img src="${val}" class="h-10 rounded shadow-sm border border-gray-200 hover:scale-110 transition-transform" alt="Photo" /></a>`;
                    }
                    
                    let tdClass = "px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-b border-gray-200";
                    if (c === 'SmsTemplate' || c === 'Location' || c === 'Items') {
                        tdClass = "px-6 py-4 whitespace-normal break-words text-sm text-gray-900 border-b border-gray-200 max-w-xs md:max-w-md";
                    }
                    html += `<td class="${tdClass}">${val}</td>`;
                });
                
                if (enableDelete) {
                    const pkValue = row[primaryKeyCol];
                    const encodedRow = encodeURIComponent(JSON.stringify(row));
                    let viewBtnHtml = '';
                    if (tableName === 'Orders') {
                        viewBtnHtml = `<button onclick="viewOrderDetails('${encodedRow}')" class="btn-icon bg-indigo-50 text-indigo-600 hover:bg-indigo-100 mr-1" title="View Details"><i class="fas fa-eye text-sm"></i></button>`;
                    }
                    html += `<td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium border-b border-gray-200">
                        ${viewBtnHtml}
                        <button onclick="editRow('${tableName}', '${primaryKeyCol}', '${encodedRow}')" class="btn-icon btn-icon-edit mr-1" title="Edit"><i class="fas fa-pen text-sm"></i></button>
                        <button onclick="deleteRow('${tableName}', '${primaryKeyCol}', '${pkValue}')" class="btn-icon btn-icon-del" title="Delete"><i class="fas fa-trash text-sm"></i></button>
                    </td>`;
                }
                html += '</tr>';
            });
            html += '</tbody></table>';
            return html;
        }

        async function loadSettings() {
            try {
                const response = await fetch('/api/admin/database');
                const data = await response.json();
                const shopsTable = data.find(t => t.tableName === 'ShopOwners');
                if (shopsTable && shopsTable.rows.length > 0) {
                    const shop = shopsTable.rows[0];
                    document.getElementById('settingShopName').value = shop.ShopName || '';
                    document.getElementById('settingShopPhone').value = shop.PhoneNumber || '';
                    document.getElementById('settingShopAddress').value = shop.Address || '';
                    document.getElementById('settingSmsTemplate').value = shop.SmsTemplate || '';
                }
            } catch (err) {}
        }

        async function loadDatabase() {
            try {
                const response = await fetch('/api/admin/database');
                const data = await response.json();
                
                let rawHtml = '<div class="space-y-8">';
                data.forEach(table => {
                    rawHtml += `
                        <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden w-full">
                            <div class="bg-gray-50 px-6 py-4 border-b border-gray-100 flex items-center">
                                <div class="bg-blue-100 p-2 rounded-lg mr-3">
                                    <i class="fas fa-database text-blue-600"></i>
                                </div>
                                <div>
                                    <h3 class="text-lg font-semibold text-gray-900">${table.tableName} Table</h3>
                                </div>
                            </div>
                            <div class="p-0 overflow-x-auto">
                                ${generateTableHtml(table.tableName, table.columns, table.rows, false)}
                            </div>
                        </div>
                    `;
                });
                rawHtml += '</div>';
                document.getElementById('raw-tables-container').innerHTML = rawHtml;

                const ordersTable = data.find(t => t.tableName === 'Orders');
                if (ordersTable) {
                    window.cachedOrdersTable = ordersTable;
                    document.getElementById('stat-total-orders').innerText = ordersTable.rows.length;
                    document.getElementById('stat-delivered').innerText = ordersTable.rows.filter(r => r.Status === 'Delivered').length;
                    document.getElementById('stat-active').innerText = ordersTable.rows.filter(r => r.Status === 'Assigned').length;
                    applyOrderFilters(); // Call this instead of generateTableHtml directly to apply current filters
                }

                const driversTable = data.find(t => t.tableName === 'Drivers');
                if (driversTable) {
                    document.getElementById('driversGrid').innerHTML = generateTableHtml('Drivers', driversTable.columns, driversTable.rows, true);
                    
                    const dSelect = document.getElementById('newOrderDriver');
                    const curVal = dSelect.value;
                    dSelect.innerHTML = '<option value="">-- Select Driver --</option>';
                    
                    const fSelect = document.getElementById('filterOrderDriver');
                    const fVal = fSelect.value;
                    fSelect.innerHTML = '<option value="">All Drivers</option>';

                    driversTable.rows.forEach(r => {
                        const optHtml = `<option value="${r.PhoneNumber}">${r.FullName} (${r.PhoneNumber})</option>`;
                        dSelect.innerHTML += optHtml;
                        fSelect.innerHTML += optHtml;
                    });
                    dSelect.value = curVal;
                    fSelect.value = fVal;
                }
            } catch (err) {}
        }

        function applyOrderFilters() {
            if (!window.cachedOrdersTable) return;
            
            const search = document.getElementById('filterOrderSearch').value.toLowerCase();
            const driver = document.getElementById('filterOrderDriver').value;
            const dateStr = document.getElementById('filterOrderDate').value; // YYYY-MM-DD
            
            let filteredRows = window.cachedOrdersTable.rows.filter(row => {
                let match = true;
                
                // Search
                if (search) {
                    const rowText = Object.values(row).join(' ').toLowerCase();
                    if (!rowText.includes(search)) match = false;
                }
                
                // Driver
                if (driver && row.DriverPhone !== driver) {
                    match = false;
                }
                
                // Date
                if (dateStr && row.OrderDate) {
                    const orderDateStr = new Date(row.OrderDate).toISOString().split('T')[0];
                    if (orderDateStr !== dateStr) match = false;
                }
                
                return match;
            });
            
            document.getElementById('ordersGrid').innerHTML = generateTableHtml('Orders', window.cachedOrdersTable.columns, filteredRows, true);
        }

        function clearOrderFilters() {
            document.getElementById('filterOrderSearch').value = '';
            document.getElementById('filterOrderDriver').value = '';
            document.getElementById('filterOrderDate').value = '';
            applyOrderFilters();
        }

        document.getElementById('updateSettingsForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const shopName = document.getElementById('settingShopName').value.replace(/'/g, "''");
            const shopPhone = document.getElementById('settingShopPhone').value.replace(/'/g, "''");
            const address = document.getElementById('settingShopAddress').value.replace(/'/g, "''");
            const smsTemplate = document.getElementById('settingSmsTemplate').value.replace(/'/g, "''");
            const sql = `UPDATE ShopOwners SET ShopName = '${shopName}', PhoneNumber = '${shopPhone}', Address = '${address}', SmsTemplate = '${smsTemplate}' WHERE Id = 1`;
            const result = await runSql(sql);
            if(result.success) {
                loadDatabase();
                lockSettings();
            }
        });

        document.getElementById('updateCredsForm').addEventListener('submit', (e) => {
            e.preventDefault();
            const oldPass = document.getElementById('credOldPass').value;
            const newUser = document.getElementById('credNewUser').value;
            const newPass = document.getElementById('credNewPass').value;
            const msgEl = document.getElementById('credMessage');
            
            const expectedPass = localStorage.getItem('adminPass') || 'admin123';
            
            if (oldPass !== expectedPass) {
                msgEl.textContent = "Current password is incorrect!";
                msgEl.className = "text-sm text-center mt-2 text-red-600 font-medium block";
                return;
            }

            if (newUser) localStorage.setItem('adminUser', newUser);
            if (newPass) localStorage.setItem('adminPass', newPass);

            msgEl.textContent = "Credentials updated successfully!";
            msgEl.className = "text-sm text-center mt-2 text-green-600 font-medium block";
            document.getElementById('updateCredsForm').reset();
            
            setTimeout(() => {
                msgEl.classList.add('hidden');
            }, 3000);
        });


        document.getElementById('createDriverForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const btn = document.getElementById('btnRegisterDriver');
            const originalText = btn.innerHTML;
            
            // Loading state
            btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Saving...';
            btn.disabled = true;

            const name = document.getElementById('newDriverName').value.replace(/'/g, "''");
            const phone = document.getElementById('newDriverPhone').value.replace(/'/g, "''");
            const password = document.getElementById('newDriverPassword').value.replace(/'/g, "''");
            const sql = `INSERT INTO Drivers (FullName, PhoneNumber, PasswordHash) VALUES ('${name}', '${phone}', '${password}')`;
            
            const result = await runSql(sql);
            
            if(result.success) {
                // Success state animation
                btn.innerHTML = '<i class="fas fa-check mr-2"></i> Success!';
                btn.style.backgroundColor = '#10B981'; // Tailwind green-500
                
                e.target.reset();
                loadDatabase();
                
                // Revert after 2 seconds
                setTimeout(() => {
                    btn.innerHTML = originalText;
                    btn.style.backgroundColor = ''; // Revert to class style
                    btn.disabled = false;
                }, 2000);
            } else {
                // Revert on failure
                btn.innerHTML = originalText;
                btn.disabled = false;
                uiAlert("Failed to add driver.");
            }
        });

        // Product Builder Logic
        const catalog = {
            Cement: ["Ultratech", "Zuari", "Ramco", "KCP", "JSW"],
            Steel: ["JR TMT", "Meenakshi", "Tata Tiscon"]
        };
        
        let currentCategory = 'Cement';
        let currentBrand = 'Ultratech';
        let currentType = 'OPC'; // Default for cement
        let builtItems = [];
        
        function selCategory(cat) {
            currentCategory = cat;
            document.getElementById('tabCement').className = cat === 'Cement' ? 'font-bold text-blue-600 border-b-2 border-blue-600 pb-1 px-2 flex items-center' : 'font-bold text-gray-500 pb-1 px-2 hover:text-gray-700 flex items-center';
            document.getElementById('tabSteel').className = cat === 'Steel' ? 'font-bold text-blue-600 border-b-2 border-blue-600 pb-1 px-2 flex items-center' : 'font-bold text-gray-500 pb-1 px-2 hover:text-gray-700 flex items-center';
            
            const typeSel = document.getElementById('typeSelection');
            if(cat === 'Cement') {
                typeSel.classList.remove('hidden');
                currentType = 'OPC';
                selType('OPC');
            } else {
                typeSel.classList.add('hidden');
                currentType = '';
            }
            
            renderBrands();
        }
        
        function renderBrands() {
            const grid = document.getElementById('brandsGrid');
            grid.innerHTML = '';
            const brands = catalog[currentCategory];
            if(!brands.includes(currentBrand)) currentBrand = brands[0];
            
            brands.forEach(b => {
                const isSel = b === currentBrand;
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = `p-3 rounded-lg border text-sm font-bold flex flex-col items-center justify-center transition-all ${isSel ? 'bg-blue-50 border-blue-500 text-blue-700 ring-1 ring-blue-500' : 'bg-white border-gray-200 text-gray-600 hover:bg-gray-50'}`;
                btn.onclick = () => { currentBrand = b; renderBrands(); };
                
                let iconHtml = currentCategory === 'Cement' ? '<i class="fas fa-cubes mb-2 text-xl opacity-50"></i>' : '<i class="fas fa-bars mb-2 text-xl opacity-50"></i>';
                btn.innerHTML = `${iconHtml} <span class="truncate w-full text-center">${b}</span>`;
                grid.appendChild(btn);
            });
        }
        
        function selType(t) {
            currentType = t;
            ['OPC','PPC','SUPER'].forEach(type => {
                const el = document.getElementById('type' + type);
                if (el) el.className = type === t ? 'px-4 py-2 border rounded-md text-sm font-bold bg-blue-600 text-white border-blue-600 shadow-sm' : 'px-4 py-2 border rounded-md text-sm font-medium bg-white text-gray-700 hover:bg-blue-50';
            });
        }
        
        function adjQty(val) {
            const el = document.getElementById('builderQty');
            let v = parseInt(el.value) + val;
            if(v < 1) v = 1;
            el.value = v;
        }
        
        function addBuiltItem() {
            const qty = document.getElementById('builderQty').value;
            let str = `${qty}x ${currentBrand} ${currentCategory}`;
            if(currentCategory === 'Cement' && currentType) str += ` (${currentType})`;
            
            builtItems.push(str);
            renderBuiltItems();
            document.getElementById('builderQty').value = 1;
        }
        
        function removeBuiltItem(index) {
            builtItems.splice(index, 1);
            renderBuiltItems();
        }
        
        function renderBuiltItems() {
            const disp = document.getElementById('selectedItemsDisplay');
            disp.innerHTML = '';
            builtItems.forEach((item, idx) => {
                disp.innerHTML += `<div class="bg-indigo-100 text-indigo-800 px-3 py-1.5 rounded-full text-sm font-medium flex items-center shadow-sm border border-indigo-200 mt-2">
                    ${item}
                    <button type="button" onclick="removeBuiltItem(${idx})" class="ml-2 text-indigo-400 hover:text-indigo-700 focus:outline-none"><i class="fas fa-times-circle text-base"></i></button>
                </div>`;
            });
            document.getElementById('newOrderItems').value = builtItems.join(', ');
        }
        
        // Initialize builder on load
        selCategory('Cement');

        document.getElementById('createOrderForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            
            if (builtItems.length === 0) {
                uiAlert("Please add at least one item to the order.");
                return;
            }
            
            const btn = document.getElementById('btnDispatchOrder');
            btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Dispatching...';
            btn.disabled = true;

            const payload = {
                DriverPhone: document.getElementById('newOrderDriver').value,
                ShopOwnerPhone: document.getElementById('settingShopPhone').value || '555-0200', 
                CustomerName: document.getElementById('newOrderCustomer').value,
                CustomerPhone: document.getElementById('newOrderPhone').value,
                Location: document.getElementById('newOrderLocation').value,
                Items: document.getElementById('newOrderItems').value
            };
            const res = await fetch('/api/orders/create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            
            btn.innerHTML = '<i class="fas fa-paper-plane mr-2"></i> Dispatch Order';
            btn.disabled = false;

            if(res.ok) {
                e.target.reset();
                builtItems = [];
                renderBuiltItems();
                loadDatabase();
                
                // Show success UI animation
                const originalClass = btn.className;
                btn.className = 'bg-green-500 text-white px-8 py-2 rounded-lg font-medium transition-all duration-300 flex items-center';
                btn.innerHTML = '<i class="fas fa-check mr-2"></i> Success!';
                setTimeout(() => {
                    btn.className = originalClass;
                    btn.innerHTML = '<i class="fas fa-paper-plane mr-2"></i> Dispatch Order';
                }, 2000);
            } else {
                uiAlert("Failed to create order: " + await res.text());
            }
        });

        loadDatabase();
        setInterval(loadDatabase, 10000);
