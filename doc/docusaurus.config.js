// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require("prism-react-renderer/themes/github");
const darkCodeTheme = require("prism-react-renderer/themes/dracula");

require("dotenv").config();

const orgName = process.env.ORG_NAME ?? "MirageNet";
const repoName = process.env.REPO_NAME ?? "Mirage";

const githubUrl = `https://github.com/${orgName}/${repoName}`;

const fs = require('fs');

if(!fs.existsSync("docs/reference/overview.md")) {
  fs.writeFileSync("docs/reference/overview.md", "# Overview")
}

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: "Mirage Networking",
  tagline: "Easy to use high performance Network library for Unity",
  url: `https://${orgName}.github.io/`,
  baseUrl: `/${repoName}/`,
  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "warn",
  favicon: "img/favicon.ico",
  organizationName: orgName, // Usually your GitHub org/user name.
  projectName: repoName, // Usually your repo name.

  presets: [
    [
      "classic",
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: require.resolve("./sidebars.js"),
          editUrl: `${githubUrl}/tree/master/doc/`,
          remarkPlugins: [require("mdx-mermaid")],
        },
        blog: false,
        theme: {
          customCss: require.resolve("./src/css/custom.scss"),
        },
      }),
    ],
  ],

  plugins: [
    require.resolve("docusaurus-lunr-search"),
    "docusaurus-plugin-sass"
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      colorMode: {
        defaultMode: "dark",
        disableSwitch: false,
        respectPrefersColorScheme: false,
      },
      navbar: {
        title: "Mirage",
        logo: {
          alt: "Mirage",
          src: "img/logo.png",
        },
        items: [
          {
            type: "doc",
            docId: "general/overview",
            position: "left",
            label: "Articles",
          },
          {
            type: "doc",
            docId: "reference/overview",
            position: "left",
            label: "API Reference",
          },
          {
            className: "navbar-github-link",
            position: "right",
            href: githubUrl,
          },
          {
            className: "navbar-discord-link",
            position: "right",
            href: "https://discord.gg/DTBPBYvexy",
          },
          // {to: '/changelog', label: 'Changelog', position: 'left'},
        ],
      },
      footer: {
        style: "dark",
        copyright: `Copyright © ${new Date().getFullYear()} Mirage, MirageNet. Built with Docusaurus.`,
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
        additionalLanguages: ["csharp", "json"],
      },
    }),
};

module.exports = config;
