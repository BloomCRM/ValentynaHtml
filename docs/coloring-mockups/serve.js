// Перегляд макетів: з кореня репо — node docs/coloring-mockups/serve.js . 5091, далі http://localhost:5091/docs/coloring-mockups/
const http = require('http'), fs = require('fs'), path = require('path');
const [root, port] = [path.resolve(process.argv[2]), +process.argv[3]];
const types = { '.html': 'text/html; charset=utf-8', '.css': 'text/css', '.js': 'text/javascript', '.jpg': 'image/jpeg', '.png': 'image/png', '.webp': 'image/webp', '.woff2': 'font/woff2', '.svg': 'image/svg+xml' };
http.createServer((req, res) => {
  const file = path.join(root, decodeURIComponent(req.url.split('?')[0]));
  if (!file.startsWith(root)) { res.writeHead(403).end(); return; }
  fs.readFile(file, (err, data) => {
    if (err) { res.writeHead(404).end('not found'); return; }
    res.writeHead(200, { 'Content-Type': types[path.extname(file).toLowerCase()] || 'application/octet-stream' }).end(data);
  });
}).listen(port, '127.0.0.1', () => console.log('serving', root, 'on', port));
