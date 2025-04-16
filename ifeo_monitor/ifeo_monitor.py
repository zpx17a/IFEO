import winreg
import time
from datetime import datetime

IFEO_PATH = r"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options"

def monitor_ifeo_changes():
    hive = winreg.HKEY_LOCAL_MACHINE
    key = winreg.OpenKey(hive, IFEO_PATH, 0, winreg.KEY_READ)
    initial_subkeys = get_subkeys(key)
    previous_debuggers = {subkey: check_debugger_value(subkey) for subkey in initial_subkeys}
    
    print(f"[{datetime.now()}] 初始子键数量: {len(initial_subkeys)}")
    
    try:
        while True:
            current_subkeys = get_subkeys(key)
            
            new_subkeys = list(set(current_subkeys) - set(initial_subkeys))
            if new_subkeys:
                for subkey in new_subkeys:
                    debugger_value = check_debugger_value(subkey)
                    log_event(f"新增子键: {subkey} | Debugger值: {debugger_value}")
                    previous_debuggers[subkey] = debugger_value  # 立即记录初始值
            
            deleted_subkeys = list(set(initial_subkeys) - set(current_subkeys))
            if deleted_subkeys:
                for subkey in deleted_subkeys:
                    log_event(f"删除子键: {subkey}")
                    previous_debuggers.pop(subkey, None)
            
            for subkey in current_subkeys:
                current_value = check_debugger_value(subkey)
                previous_value = previous_debuggers.get(subkey)
                
                if previous_value is None:
                    continue
                if current_value != previous_value:
                    log_event(f"子键 '{subkey}' 修改Debugger: {previous_value} → {current_value}")
                    previous_debuggers[subkey] = current_value
            
            initial_subkeys = current_subkeys
            time.sleep(1)
            
    except KeyboardInterrupt:
        print("监控已停止")
    finally:
        winreg.CloseKey(key)

def get_subkeys(key):
    subkeys = []
    try:
        num_subkeys = winreg.QueryInfoKey(key)[0]
        for i in range(num_subkeys):
            subkey_name = winreg.EnumKey(key, i)
            subkeys.append(subkey_name)
    except OSError as e:
        print(f"读取子键失败: {e}")
    return subkeys

def check_debugger_value(subkey_name):
    subkey_path = f"{IFEO_PATH}\\{subkey_name}"
    try:
        key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, subkey_path, 0, winreg.KEY_READ)
        debugger_value, _ = winreg.QueryValueEx(key, "Debugger")
        winreg.CloseKey(key)
        return debugger_value
    except FileNotFoundError:
        return "未设置Debugger"
    except PermissionError:
        return "权限不足"
    except Exception as e:
        return f"读取失败: {str(e)}"

def log_event(message):
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    log_line = f"[{timestamp}] {message}\n"
    print(log_line, end='')
    with open("ifeo_monitor.log", "a", encoding="utf-8") as f:
        f.write(log_line)

if __name__ == "__main__":
    monitor_ifeo_changes()
