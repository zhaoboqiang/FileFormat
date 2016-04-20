# FileFormat
Format file, such as file line ending into unix(LF) and encoding into utf8.

For a example:

// Inplace format *.h,*cpp,*.c
FileFormat -i input_root_dir

// Format *.h,*.cpp,*.c from input root dir to output root dir
FileFormat -i input_root_dir -o output_root_dir

// Format *.txt,*.csv
FileFormat -i input_root_dir -e *.txt,*.csv
