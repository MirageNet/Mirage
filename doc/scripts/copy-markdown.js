const fs = require('fs');
const path = require('path');

// Load .env configuration to match docusaurus.config.js
require('dotenv').config();

// Configure base URL from environment or fallback to repository defaults
const orgName = process.env.ORG_NAME ?? 'MirageNet';
const repoName = process.env.REPO_NAME ?? 'Mirage';
const baseUrl = `https://${orgName}.github.io/${repoName}`;

const sourceDir = path.resolve(__dirname, '..', 'docs');
const targetDocsDir = path.resolve(__dirname, '..', 'build', 'docs');
const targetBuildDir = path.resolve(__dirname, '..', 'build');

// Capitalize category folder names for display headings
function formatCategoryName(folder) {
    const customNames = {
        general: 'General',
        guides: 'Guides',
        components: 'Components',
        examples: 'Examples',
        reference: 'API Reference',
    };
    return customNames[folder] || folder.charAt(0).toUpperCase() + folder.slice(1);
}

// Extract human-readable title from frontmatter or first markdown heading in body
function extractTitle(content, fileName) {
    const frontmatterMatch = content.match(/^---\r?\n([\s\S]*?)\r?\n---/);
    if (frontmatterMatch) {
        const titleMatch = frontmatterMatch[1].match(/(?:title|sidebar_label):\s*([^\r\n]+)/);
        if (titleMatch) {
            return titleMatch[1].replace(/^["']|["']$/g, '').trim();
        }
    }

    // Strip frontmatter block before matching body heading to avoid matching YAML comments
    const body = content.replace(/^---\r?\n[\s\S]*?\r?\n---/, '');
    const headingMatch = body.match(/^#\s+(.+)$/m);
    if (headingMatch) {
        return headingMatch[1].trim();
    }

    return path.basename(fileName, path.extname(fileName))
        .replace(/[-_]/g, ' ')
        .replace(/\b\w/g, char => char.toUpperCase());
}

// Recursively copy markdown files and index them for agent consumption
function processDocs(src, dest, collectedDocs = [], relativePrefix = '') {
    if (!fs.existsSync(src)) {
        return collectedDocs;
    }

    const entries = fs.readdirSync(src, { withFileTypes: true });

    for (const entry of entries) {
        const srcPath = path.join(src, entry.name);
        const destPath = path.join(dest, entry.name);
        const relPath = relativePrefix ? `${relativePrefix}/${entry.name}` : entry.name;

        if (entry.isDirectory()) {
            processDocs(srcPath, destPath, collectedDocs, relPath);
        } else if (entry.isFile() && (entry.name.endsWith('.md') || entry.name.endsWith('.mdx'))) {
            fs.mkdirSync(path.dirname(destPath), { recursive: true });
            fs.copyFileSync(srcPath, destPath);

            const content = fs.readFileSync(srcPath, 'utf8');
            const topCategory = relPath.includes('/') ? relPath.split('/')[0] : 'general';
            const title = extractTitle(content, entry.name);

            collectedDocs.push({
                title,
                category: topCategory,
                relPath: relPath.replace(/\\/g, '/'),
            });
        }
    }

    return collectedDocs;
}

// Generate an llms.txt sitemap according to the llmstxt.org standard
function generateLlmsTxt(docs) {
    const lines = [
        '# Mirage Networking',
        '',
        '> High-performance, modular networking library for Unity.',
        '',
        `- Quick Cheat Sheet / Skill: ${baseUrl}/skill.md`,
        '',
        '## Documentation Index',
        '',
    ];

    const grouped = {};
    for (const doc of docs) {
        if (!grouped[doc.category]) {
            grouped[doc.category] = [];
        }
        grouped[doc.category].push(doc);
    }

    // Ensure common categories appear in intuitive reading order
    const orderedCategories = ['general', 'guides', 'components', 'examples', 'reference'];
    const allCategories = Array.from(new Set([...orderedCategories, ...Object.keys(grouped)]));

    for (const cat of allCategories) {
        const items = grouped[cat];
        if (!items || items.length === 0) continue;

        lines.push(`### ${formatCategoryName(cat)}`);
        for (const item of items) {
            lines.push(`- [${item.title}](${baseUrl}/docs/${item.relPath})`);
        }
        lines.push('');
    }

    return lines.join('\n');
}

const collectedDocs = processDocs(sourceDir, targetDocsDir);

if (fs.existsSync(targetBuildDir) && collectedDocs.length > 0) {
    const llmsContent = generateLlmsTxt(collectedDocs);

    // Save llms.txt at root of build for standard AI discovery: https://miragenet.github.io/Mirage/llms.txt
    fs.writeFileSync(path.join(targetBuildDir, 'llms.txt'), llmsContent, 'utf8');

    // Also write a markdown sitemap inside docs folder
    fs.writeFileSync(path.join(targetDocsDir, 'sitemap.md'), llmsContent, 'utf8');

    // Copy skill.md to build root so https://miragenet.github.io/Mirage/skill.md is directly accessible
    const skillPath = path.join(sourceDir, 'guides', 'skill.md');
    if (fs.existsSync(skillPath)) {
        fs.copyFileSync(skillPath, path.join(targetBuildDir, 'skill.md'));
    }
}
