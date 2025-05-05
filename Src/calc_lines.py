import os

def calc_file(path: str) -> tuple[int, int]:
    with open(path, "rb") as f:
        lines_cnt = len(f.readlines())
        bytes_cnt = f.tell()
    return (lines_cnt, bytes_cnt)

def visit_dir(path: str) -> list[tuple[int, int]]:
    """
    Visit the all csharp files in the specified directory
    and its subdirectories.
    """
    result = []
    for root, _, files in os.walk(path):
        if any([x in root for x in ["\\bin", "\\obj"]]):
            continue
        #print(f"Visiting {root}...")
        for file in files:
            if file.endswith(".cs"):
                file_path = os.path.join(root, file)
                result.append(calc_file(file_path))
    return result

def main():
    paths = [
        "./KimerA/Assets/Plugins/KimerA",
        "./KimerA.Analysis",
        "./KimerA.Analysis.ECS",
    ]
    total = (0, 0)
    count = 0
    for path in paths:
        result = visit_dir(path)
        lines_sum, bytes_sum = map(sum, zip(*result))
        print(f">>> {path} <<")
        print(f"    Lines: {lines_sum}")
        print(f"    Bytes: {bytes_sum} ({bytes_sum / 1024:.2f} KB)")
        print(f"    Files: {len(result)}")
        total = (total[0] + lines_sum, total[1] + bytes_sum)
        count += len(result)
    print(">>> All <<")
    print(f"    Total lines: {total[0]}")
    print(f"    Total bytes: {total[1]} ({total[1] / 1024:.2f} KB)")
    print(f"    Total directories: {len(paths)}")
    print(f"    Total files: {len(result)}")

if __name__ == "__main__":
    main()
