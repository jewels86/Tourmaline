# Tourmaline-Directory Enumerator (v1.0)
A directory enumeration tool with many modes.

## Installation
This installation process needs refinement, but this works for now.
### Linux
Download the newest release's `linux-x64.zip` and extract it. Then run the command `source init.sh` from inside the directory, then `source ~/.bashrc`. This sets the alias so the app is ready to be used.
### Windows
Download the newest release's `win-x64.zip` and extract it. Then run the command `./init.ps1` from inside the directory. This sets the alias so the app is ready to be used.

## Usage
### Commands
Tourmaline currently has 3 commands:
- `spider`: Starts tourmaline in spider mode
- `brute`: Starts tourmaline in brute force mode
- `build`: Stargs tourmaline in command builder mode
### Spider
The tourmaline spider is used to find files used in the source code of a site. Here's an explaination of it's workings:
1. The spider sends a request to the page given.
2. The spider scans the page for paths and links and adds them to the queue.
3. The spider then iterates through the queue and finds more links and paths as it goes.
Then it returns all paths found.  

Flags:
- `-m|--max-paths`: Sets the max amount of paths to find and iterate through.
- `-r`: The regex all paths must pass to be returned.
- `-i`: The regex all paths must NOT pass to be returned (ignored)
Other flags are specified in the `tourmaline spider -h` command.
### Brute
The tourmaline brute forcer is used to find pages served by the site. It needs a wordlist to function. It pretty much just sends requests to every path on the wordlist and logs successful attempts.
### Build
The build command is used to generate commands for tourmaline. You enter some information about the purpose and it gives you a command.

## Todos
- Spider
    - Add threads
    - Add stray value 
    - Speed optimizations
- Brute
    - Add threads
- Add docs
- Installations
    - Add package to `apt`
    - Fix windows installation