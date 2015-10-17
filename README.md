# BPMplaylist
Usage: bpmplaylist -d directory [OPTIONS]
Creates a pseudorandom playlist based on your music's BPM and playlist lenght.
If no options has been set, a playlist spanning 45 minutes will be created.

Options:
  -d, --directory=DIRECTORY  the DIRECTORY with your music files.
                               it is run in the current directory if none is
                               given.
  -O, --output=FILENAME      the output FILENAME, should be a m3u file, to work
                               with mpd.
  -t, --timespan=VALUE       the timespan of the playlist.
  -l, --maxlength=VALUE      the max length of chosen songs.
  -s, --silent               silent or quiet mode.
  -g, --granularity=VALUE    the granularity of the BPM groups.
                               more granularity gives less randomness.
                               this must be an integer.
  -h, --help                 show this message and exit
