const fs = require('fs');
const html = fs.readFileSync('index.html', 'utf8');
let code = fs.readFileSync('aws-lambda-backend.mjs', 'utf8');
// Replace the existing let html = `...`; block
code = code.replace(/let html = `[\s\S]*?`;/, 'let html = `' + html.replace(/`/g, '\\`').replace(/\$/g, '\\$') + '`;');
fs.writeFileSync('aws-lambda-backend.mjs', code);
