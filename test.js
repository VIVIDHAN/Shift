    <script>
        // Simple Auth Check
        if (sessionStorage.getItem('adminLoggedIn') !== 'true') {
            window.location.href = '/index.html';
        }
    </script>
    <script>
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
    </script>
    <script>
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
                    div.innerHTML = `
                        <label class="block text-sm font-medium text-gray-700 mb-1">${f.key}</label>
                        <input type="text" id="editField_${f.key}" value="${f.value}" class="w-full border-gray-300 rounded-md shadow-sm focus:border-blue-500 focus:ring focus:ring-blue-200 focus:ring-opacity-50 text-gray-700 p-2 border" />
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

        function generateTableHtml(tableName, columns, rows, enableDelete = true) {
            if (rows.length === 0) return '<div class="text-center p-6 text-gray-500 bg-gray-50 rounded-lg">No data found.</div>';
            
            const hiddenColumns = ['ShopOwnerPhone', 'PasswordHash'];
            const visibleCols = columns.filter(c => !hiddenColumns.includes(c));

            let html = '<table><thead><tr>';
            visibleCols.forEach(c => html += `<th>${c}</th>`);
            if (enableDelete) html += `<th class="text-right">Actions</th>`;
            html += '</tr></thead><tbody>';
            
            const primaryKeyCol = columns[0];

            [...rows].reverse().forEach(row => {
                html += '<tr>';
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
                    html += `<td>${val}</td>`;
                });
                
                if (enableDelete) {
                    const pkValue = row[primaryKeyCol];
                    const encodedRow = encodeURIComponent(JSON.stringify(row));
                    html += `<td class="text-right">
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
                
                let rawHtml = '';
                data.forEach(table => {
                    rawHtml += `<h5 class="mt-8 mb-4 font-bold text-primary">${table.tableName}</h5>`;
                    rawHtml += generateTableHtml(table.tableName, table.columns, table.rows, false);
                });
                document.getElementById('raw-tables-container').innerHTML = rawHtml;

                const ordersTable = data.find(t => t.tableName === 'Orders');
                if (ordersTable) {
                    document.getElementById('stat-total-orders').innerText = ordersTable.rows.length;
                    document.getElementById('stat-delivered').innerText = ordersTable.rows.filter(r => r.Status === 'Delivered').length;
                    document.getElementById('stat-active').innerText = ordersTable.rows.filter(r => r.Status === 'Assigned').length;
                    document.getElementById('ordersGrid').innerHTML = generateTableHtml('Orders', ordersTable.columns, ordersTable.rows, true);
                }

                const driversTable = data.find(t => t.tableName === 'Drivers');
                if (driversTable) {
                    document.getElementById('driversGrid').innerHTML = generateTableHtml('Drivers', driversTable.columns, driversTable.rows, true);
                    const dSelect = document.getElementById('newOrderDriver');
                    const curVal = dSelect.value;
                    dSelect.innerHTML = '<option value="">-- Select Driver --</option>';
                    driversTable.rows.forEach(r => dSelect.innerHTML += `<option value="${r.PhoneNumber}">${r.FullName} (${r.PhoneNumber})</option>`);
                    dSelect.value = curVal;
                }
            } catch (err) {}
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

        document.getElementById('createOrderForm').addEventListener('submit', async (e) => {
            e.preventDefault();
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
            if(res.ok) {
                e.target.reset();
                loadDatabase();
            } else {
                uiAlert("Failed to create order: " + await res.text());
            }
        });

        loadDatabase();
        setInterval(loadDatabase, 10000);
    </script>
