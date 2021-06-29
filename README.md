# N.I.N.A. plugins by Dale Ghent

This repository is a collection of plugins for [N.I.N.A.](https://nighttime-imaging.eu/)

Plugins available:

## Utilities for Astro-Physics mounts

This plugin implements functionality in N.I.N.A.'s Advanced Sequencer for doing various things with Astro-Physics mounts and APCC Pro. It currently implements the following functions:

### Create APPM Model (Sequence Instruction)

**Create APPM Model** is a sequence instruction that will run Astro-Physics Point Mapper (APPM) in an automated mode. When ran, APPM will use its existing default settings to run a point mapping session and will load the results into APCC Pro when complete. If the default settings and point map are not desired, an APPM settings or a point map file may be optionally specified below in this plugin's settings.
    
**Create APPM Model** has one runtime option: Keep APPM open. Switching this to On will keep APPM open after it completes. Be aware that the sequence will not progress until APPM is closed.

To use this instruction, you must have Enable Server set to On under Options > General in NINA and have 'NINA' set as the camera in APPM.