# Tourmaline
A comprehensive and effcient web enumeration toolkit.

![tourmaline-2](https://github.com/user-attachments/assets/3be40f92-b47f-4828-aa77-8b82398a044b)

## Overview
**Tourmaline** is a command line tool for web penetration testing. 
Tourmaline can be used to find directories, identify a site's CMS, and scrape websites for both files and data.

## Installation
**Tourmaline** is currently only available for systems supporting debian packages. Download the latest Tourmaline `.deb` file and install it with:
```
sudo dpkg -i [FILE_NAME]
```
Where `[FILE_NAME]` is the name of the downloaded file.

## Commands
### Global Flags
- `-t|--threads <THREADS>`: The number of threads to use (defaults to `12`)
- `-o|--outfile <OUTFILE-PATH>`: The path to the outfile (defaults to none)
- `--debug`: Used to debug Tourmaline (defaults to `false`)
### Spider
Tourmaline's spider creates a queue from links on pages, which it then iterates through to find more paths.

Command: `tourmaline spider <URL>`

Flags:
- `-d|--depth <MAX-DEPTH>`: The max depth the paths can reach.
- `-r|--regex <REGEX>`: A regex all paths must match to be added to the output. (Note: paths will still be added to the queue.)
- `-i|--ignore <IGNORE>`: A regex paths must not match to be added to the output. (Note: paths will still be added to the queue.)
- `--force-regex`: Paths not matching the `-r` regex will not be added to the queue.
- `--force-ignore`: Paths matching the `-i` regex will not be added to the queue.
- `-k|--known <PATHS>`: Paths that should initially be added to the queue (comma-separated)

### Brute
To brute force directories, Tourmaline reads a wordlist and tries every path listed.

Command: `tourmaline brute <URL> [WORDLIST]`

Flags:
- `-d|--depth`: The number of times to recurse through found directories (best used with smaller wordlists)

### CMS Detection
**CMS Detection** refers to the process of identifying the content manager service of a website (wordpress, joomla, drupal).

Command: `tourmaline cms <URL>`

Note: this command has no other flags. It also doesn't use the `threads` global flag.

### Scan
The scan command is a combination of the spider, CMS, and brute commands. It first identifies the CMS, uses the brute to find directories, and then uses the spider to do a final sweep.

Command: `tourmaline scan <URL>`

Flags:
- `-w|--wordlist <WORDLIST>`: The wordlist to use for brute forcing.

### File Scraper
Scrapes files from a list of paths (which can be obtained through the spider or brute).

Command: `tourmaline fscrape <url> <paths-file>`

No flags so far.

### Data Scraper
Scrapes data from a list of pages.

Command: `tourmaline dscrape <url> <paths-file> <regex>`

No flags so far.

## Gallery
![tourmaline-1](https://github.com/user-attachments/assets/ad1d5082-c603-451a-bb92-d924b87ebc17)
![tourmaline-3](https://github.com/user-attachments/assets/b6c5fcd7-9bc0-40d6-853c-da50e45059ee)

## Todos
- Add program to `apt` and `winget`
- Add some pictures to readme
- Add a `--version` flag
- Add squarespace and wix to CMS detection
- Add `--no-`something flags to scan 
- Add data scraper flags like get surounding characters/current tag
- Fix CMS detection command to not use a preparation spinner
- Switch license to MIT
- Fix weird duplicates in spider
