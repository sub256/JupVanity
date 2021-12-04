# JUP Vanity Address Generator

## How to use:

--beginswith, -b:
Search for specific text at the beginning of the address. The numbers 0, 1 and letters I, O cannot be used in the address. 

```
JupVanity.exe -b BEST

Example of Address Found: JUP-BEST-E5S8-7L94-26L35
```

--endswith, -e:
Search for specific text at the end of the address. The numbers 0, 1 and letters I, O cannot be used in the address.
```
JupVanity.exe -e GREEN

Example of Address Found: JUP-7UJ5-YP4Z-V973-GREEN
```

For more options...
```
JupVanity.exe --help
```

Once an address has been found, a file named jupvanity.txt will be created in the same folder as the application.
This file contains the generated address and passphrase required to access it. If this file already exists, results will be appended.


If you've found this application useful, tips can be sent to - JUP-GG8Y-7CAS-4G9A-77777

Thank you to the original author who was kind enough to share the source and make this possible - NXT: 1102622531
