import sys
import os
from datetime import datetime
from Utils import CompressDotBimFile,DeCompressDotBimFile
from dotbimpy import File
def main():
    args = sys.argv[1:]

    if len(args) < 3:
        print("Usage: python run.py <filePath> <output_folder> <option:com|deCom>")
        return

    filePath = args[0]
    output = args[1]
    option = args[2]

    if output is None:
        output = "./output"

    if not os.path.exists(filePath):
        print(f"File '{filePath}' not found.")
        return

    if not os.path.exists(output):
        try:
            os.makedirs(output)
            print(f"Directory '{output}' created successfully.")
        except Exception as ex:
            print(f"Error creating directory: {ex}")
            return

    if option is None:
        print("Missing option")
        return

    start_time = datetime.now()
    file_name = os.path.splitext(os.path.basename(filePath))[0]

    if option == "com":
        try:
            file =File.read(filePath)
            dot= CompressDotBimFile(file)
            compressed_bytes= dot.compress()
            with open(os.path.join(output, f"{file_name}-python-com.gz"), 'wb') as compressed_file:
                compressed_file.write(compressed_bytes)
            print(f"Compress python completed in {((datetime.now() - start_time).total_seconds())} seconds.")
        except Exception as ex:
            raise Exception(str(ex))

    elif option == "deCom":
        try:
            with open(filePath, 'rb') as compressed_file:
                compressed_bytes = compressed_file.read()
                decompressed = DeCompressDotBimFile(compressed_bytes)
                decompressed_file=decompressed.de_compress()
                decompressed_file.save(os.path.join(output, f"{file_name}-python-deCom.bim"))

            print(f"DeCompress python completed in {((datetime.now() - start_time).total_seconds())} seconds.")
        except Exception as ex:
            raise Exception(str(ex))

    else:
        raise Exception("Option must be 'com' or 'deCom'")

if __name__ == "__main__":
    main()