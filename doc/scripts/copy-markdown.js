const fs = require('fs');
const path = require('path');

// Raw markdown files are copied to the build directory so AI agents and CLI tools
// can directly fetch markdown content from https://miragenet.github.io/Mirage/docs/<path>.md
const sourceDir = path.resolve(__dirname, '..', 'docs');
const targetDir = path.resolve(__dirname, '..', 'build', 'docs');

function copyMarkdownFiles(src, dest) {
    if (!fs.existsSync(src)) {
        return;
    }

    const entries = fs.readdirSync(src, { withFileTypes: true });

    for (const entry of entries) {
        const srcPath = path.join(src, entry.name);
        const destPath = path.join(dest, entry.name);

        if (entry.isDirectory()) {
            copyMarkdownFiles(srcPath, destPath);
        } else if (entry.isFile() && (entry.name.endsWith('.md') || entry.name.endsWith('.mdx'))) {
            fs.mkdirSync(path.dirname(destPath), { recursive: true });
            fs.copyFileSync(srcPath, destPath);
        }
    }
}

copyMarkdownFiles(sourceDir, targetDir);
