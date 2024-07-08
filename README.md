# Tourmaline
A tunable all-in-one directory enumeration tool.

![image](https://github.com/Gold-Team-Projects/Tourmaline-Directory_Enumerator/assets/165838882/02c94d21-d65f-4f00-9c5e-f5d28e9f94c0)

Latest release: v1.18
## Credits
`wordlist.txt` is from [`dirb common.txt`](https://github.com/3ndG4me/KaliLists/blob/master/dirb/common.txt).
## Installation
This installation process needs refinement, but this works for now.
### Linux
Download the newest release's `linux-x64.zip` and extract it. Then run the command `source init.sh` from inside the directory, then `source ~/.bashrc`. This sets the alias so the app is ready to be used.
### Windows
Download the newest release's `win-x64.zip` and extract it. Then run the command `./init.ps1` from inside the directory. This sets the alias so the app is ready to be used.
## Uninstallation
### Linux
Still working on it. In the mean time, you can remove the directory and remove its path from ~/.bashrc
### Windows
Run the command `./uninstall.ps1`, then remove the directory.
## Usage
### Commands
Tourmaline currently has 4 commands:
- `spider`: Starts tourmaline in spider mode
- `brute`: Starts tourmaline in brute force mode
- `build`: Starts tourmaline in command builder mode
- `enumerate`: Combines supported methods to find as many paths as possible
### Spider
The spider is used to find files used in the source code of a site. Here's a basic explanation of its workings:
1. The spider sends a request to the page given.
2. The spider scans the page for paths and links and adds them to the queue.
3. The spider then iterates through the queue and finds more links and paths as it goes.
Then it returns all paths found.  
![image](https://github.com/Gold-Team-Projects/Tourmaline-Directory_Enumerator/assets/165838882/d8290b6e-c577-4364-a260-cda25afcf8af)


#### Flags
- `-m`: Sets the max amount of paths to find and iterate through.
- `-r`: All files with names that don't match this regex are ignored.
- `-i`: All files with names matching this regex are ignored.
- `-o`: The file to write results to.
- `-t`: The amount of threads to use. (4)
Other options are specified in the `tourmaline spider -h` command.

### Brute
The brute forcer is used to find pages served by the site. It needs a wordlist to function.
It pretty much just sends requests to every path on the wordlist and logs successful attempts. 


Flags:
- `-t`: The number of threads to use during enumeration. (4)
- `-o`: The file to write results to.

### Enumerate
This command combines many methods to find many paths.

### Build
The build command is used to generate commands for tourmaline. You enter some information about the purpose and it gives you a command.

This isn't finished yet.
## Todos
- Finish builder
- Add docs
- - Make sure docs are accurate
- Installations
    - Add package to `apt`
- Build new release
