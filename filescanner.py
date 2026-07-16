import tkinter as tk
from tkinter import ttk, filedialog, scrolledtext, messagebox
import os
import threading
import shutil
from datetime import datetime
import glob
import string

class FileScannerApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Fast Priority Scanner - Sensitive Files")
        self.root.geometry("1150x880")
        self.root.configure(bg='#1e1e1e')
        
        self.output_folder = ""
        self.processed_files = 0
        self.copied_files = 0
        self.is_scanning = False
        
        self.create_widgets()

    def create_widgets(self):
        bg = '#1e1e1e'
        fg = 'white'
        
        # Search terms
        tk.Label(self.root, text="Search terms / phrases:", bg=bg, fg=fg).grid(row=0, column=0, sticky='w', padx=20, pady=(20,5))
        self.txt_search = tk.Text(self.root, height=2, width=110, bg='#2d2d2d', fg=fg, insertbackground=fg)
        self.txt_search.grid(row=1, column=0, columnspan=6, padx=20, pady=5, sticky='ew')
        self.txt_search.insert('1.0', "mercury reconstruct, exact phrase here, password")
        
        help_text = "Separate terms with commas (,). Spaces are preserved.\nExample: mercury reconstruct, full sentence here, report2025"
        tk.Label(self.root, text=help_text, bg=bg, fg='#ffcc00', justify='left').grid(row=2, column=0, columnspan=6, sticky='w', padx=20)

        # Output
        tk.Label(self.root, text="Output Folder:", bg=bg, fg=fg).grid(row=3, column=0, sticky='w', padx=20, pady=(10,5))
        self.txt_output_path = tk.Entry(self.root, width=90, bg='#2d2d2d', fg=fg, state='readonly')
        self.txt_output_path.grid(row=4, column=0, columnspan=4, padx=20, pady=5, sticky='ew')
        tk.Button(self.root, text="Browse", command=self.browse_output, bg='#444', fg=fg).grid(row=4, column=4, padx=5)

        # Scan options
        self.chk_scan_all = tk.BooleanVar(value=True)
        tk.Checkbutton(self.root, text="Scan All Drives (Fixed + USB/External)", variable=self.chk_scan_all, 
                      bg=bg, fg=fg, selectcolor='#333', command=self.toggle_scan_mode).grid(row=5, column=0, sticky='w', padx=20, pady=5)
        
        self.chk_deep_search = tk.BooleanVar(value=False)
        tk.Checkbutton(self.root, text="Deep Search (include hidden files & folders)", variable=self.chk_deep_search, 
                      bg=bg, fg=fg, selectcolor='#333').grid(row=6, column=0, sticky='w', padx=20, pady=5)

        # Specific folder
        tk.Label(self.root, text="Specific Folder:", bg=bg, fg=fg).grid(row=7, column=0, sticky='w', padx=20, pady=(10,5))
        self.txt_scan_path = tk.Entry(self.root, width=90, bg='#2d2d2d', fg=fg, state='readonly')
        self.txt_scan_path.grid(row=8, column=0, columnspan=4, padx=20, pady=5, sticky='ew')
        self.btn_scan_browse = tk.Button(self.root, text="Browse", command=self.browse_scan, bg='#444', fg=fg, state='disabled')
        self.btn_scan_browse.grid(row=8, column=4, padx=5)

        # Search mode & size
        tk.Label(self.root, text="Search Mode:", bg=bg, fg=fg).grid(row=9, column=0, sticky='w', padx=20, pady=(15,5))
        self.chk_scan_by_filename = tk.BooleanVar(value=False)
        tk.Checkbutton(self.root, text="Scan by Filename (instead of content)", variable=self.chk_scan_by_filename, 
                      bg=bg, fg=fg, selectcolor='#333').grid(row=9, column=1, sticky='w')

        tk.Label(self.root, text="Max File Size:", bg=bg, fg=fg).grid(row=10, column=0, sticky='w', padx=20, pady=5)
        self.cmb_size_limit = ttk.Combobox(self.root, values=["3 KB", "100 KB", "1 MB", "20 MB", "200 MB"], width=15, state="readonly")
        self.cmb_size_limit.set("3 KB")
        self.cmb_size_limit.grid(row=10, column=1, sticky='w', padx=5)

        # File types
        tk.Label(self.root, text="File Types:", bg=bg, fg=fg).grid(row=11, column=0, sticky='w', padx=20, pady=(15,5))
        
        # Row 1
        row1 = 12
        self.chk_txt = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".txt", variable=self.chk_txt, bg=bg, fg=fg, selectcolor='#333').grid(row=row1, column=0, sticky='w', padx=130)
        self.chk_csv = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".csv", variable=self.chk_csv, bg=bg, fg=fg, selectcolor='#333').grid(row=row1, column=0, sticky='w', padx=210)
        self.chk_json = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".json", variable=self.chk_json, bg=bg, fg=fg, selectcolor='#333').grid(row=row1, column=0, sticky='w', padx=290)
        self.chk_key = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".key", variable=self.chk_key, bg=bg, fg=fg, selectcolor='#333').grid(row=row1, column=0, sticky='w', padx=370)
        self.chk_pem = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".pem", variable=self.chk_pem, bg=bg, fg=fg, selectcolor='#333').grid(row=row1, column=0, sticky='w', padx=450)
        self.chk_log = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".log", variable=self.chk_log, bg=bg, fg=fg, selectcolor='#333').grid(row=row1, column=0, sticky='w', padx=530)
        self.chk_xlsx = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".xlsx", variable=self.chk_xlsx, bg=bg, fg=fg, selectcolor='#333').grid(row=row1, column=0, sticky='w', padx=610)

        # Row 2
        row2 = 13
        self.chk_config = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".config", variable=self.chk_config, bg=bg, fg=fg, selectcolor='#333').grid(row=row2, column=0, sticky='w', padx=130)
        self.chk_ini = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".ini", variable=self.chk_ini, bg=bg, fg=fg, selectcolor='#333').grid(row=row2, column=0, sticky='w', padx=220)
        self.chk_xml = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".xml", variable=self.chk_xml, bg=bg, fg=fg, selectcolor='#333').grid(row=row2, column=0, sticky='w', padx=290)
        self.chk_env = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".env", variable=self.chk_env, bg=bg, fg=fg, selectcolor='#333').grid(row=row2, column=0, sticky='w', padx=360)
        self.chk_conf = tk.BooleanVar(value=True); tk.Checkbutton(self.root, text=".conf", variable=self.chk_conf, bg=bg, fg=fg, selectcolor='#333').grid(row=row2, column=0, sticky='w', padx=430)

        # Control buttons
        btn_frame = tk.Frame(self.root, bg=bg)
        btn_frame.grid(row=14, column=0, columnspan=6, pady=20, padx=20, sticky='w')
        tk.Button(btn_frame, text="Start Scan", command=self.start_scan, bg='#1e90ff', fg='white', width=15, height=2).pack(side='left', padx=5)
        tk.Button(btn_frame, text="Stop", command=self.stop_scan, bg='#ff4500', fg='white', width=12, height=2).pack(side='left', padx=5)
        tk.Button(btn_frame, text="Clear Log", command=self.clear_log, bg='#666', fg='white', width=12, height=2).pack(side='left', padx=5)

        # Progress & Status
        self.progress = ttk.Progressbar(self.root, mode='indeterminate', length=1100)
        self.progress.grid(row=15, column=0, columnspan=6, padx=20, pady=10, sticky='ew')
        
        self.lbl_status = tk.Label(self.root, text="Ready", bg=bg, fg='#00ff00', font=('Segoe UI', 10))
        self.lbl_status.grid(row=16, column=0, columnspan=6, sticky='w', padx=20)

        # Log
        self.log_box = scrolledtext.ScrolledText(self.root, height=15, bg='#141414', fg='#ddd', font=('Consolas', 10))
        self.log_box.grid(row=17, column=0, columnspan=6, padx=20, pady=10, sticky='nsew')

        self.root.grid_rowconfigure(17, weight=1)
        self.root.grid_columnconfigure(0, weight=1)

    def toggle_scan_mode(self):
        state = 'disabled' if self.chk_scan_all.get() else 'normal'
        self.btn_scan_browse.config(state=state)

    def browse_output(self):
        folder = filedialog.askdirectory(title="Select Output Folder")
        if folder:
            self.output_folder = folder
            self.txt_output_path.config(state='normal')
            self.txt_output_path.delete(0, tk.END)
            self.txt_output_path.insert(0, folder)
            self.txt_output_path.config(state='readonly')

    def browse_scan(self):
        folder = filedialog.askdirectory(title="Select Scan Folder")
        if folder:
            self.txt_scan_path.config(state='normal')
            self.txt_scan_path.delete(0, tk.END)
            self.txt_scan_path.insert(0, folder)
            self.txt_scan_path.config(state='readonly')

    def log(self, message, color='white'):
        timestamp = datetime.now().strftime("%H:%M:%S")
        self.log_box.configure(state='normal')
        self.log_box.insert(tk.END, f"[{timestamp}] {message}\n")
        self.log_box.see(tk.END)
        self.log_box.configure(state='disabled')
        self.root.update_idletasks()

    def get_extensions(self):
        exts = []
        if self.chk_txt.get(): exts.append('.txt')
        if self.chk_csv.get(): exts.append('.csv')
        if self.chk_json.get(): exts.append('.json')
        if self.chk_key.get(): exts.append('.key')
        if self.chk_pem.get(): exts.append('.pem')
        if self.chk_log.get(): exts.append('.log')
        if self.chk_xlsx.get(): exts.append('.xlsx')
        if self.chk_config.get(): exts.append('.config')
        if self.chk_ini.get(): exts.append('.ini')
        if self.chk_xml.get(): exts.append('.xml')
        if self.chk_env.get(): exts.append('.env')
        if self.chk_conf.get(): exts.append('.conf')
        return exts

    def get_max_size(self):
        size_str = self.cmb_size_limit.get()
        num = int(size_str.split()[0])
        return num * (1024 if 'KB' in size_str else 1024*1024)

    def should_skip_dir(self, path):
        lower = path.lower()
        skips = ['\\windows\\', '\\program files', '\\programdata\\', '\\appdata\\local\\temp', '\\system32\\', '\\syswow64\\']
        return any(s in lower for s in skips)

    def scan_directory(self, path, keywords, extensions, max_size):
        if not os.path.exists(path) or self.should_skip_dir(path):
            return
        if self.output_folder and path.lower().startswith(self.output_folder.lower()):
            return

        try:
            for ext in extensions:
                for file in glob.glob(os.path.join(path, f"*{ext}")):
                    self.processed_files += 1
                    self.update_status()

                    try:
                        if os.path.getsize(file) > max_size:
                            continue
                        if not self.chk_deep_search.get() and (os.stat(file).st_file_attributes & 2):  # Hidden
                            continue

                        match = False
                        if self.chk_scan_by_filename.get():
                            match = any(k in os.path.basename(file).lower() for k in keywords)
                        else:
                            try:
                                with open(file, 'r', encoding='utf-8', errors='ignore') as f:
                                    content = f.read().lower()
                                match = any(k in content for k in keywords)
                            except:
                                continue

                        if match:
                            dest = os.path.join(self.output_folder, os.path.basename(file))
                            base, ext = os.path.splitext(os.path.basename(file))
                            counter = 1
                            while os.path.exists(dest):
                                dest = os.path.join(self.output_folder, f"{base}_{counter}{ext}")
                                counter += 1
                            shutil.copy2(file, dest)
                            self.copied_files += 1
                            self.log(f"✓ Copied: {file} ({os.path.getsize(file)//1024} KB)", 'lime')
                    except:
                        pass
        except:
            pass

        # Recurse
        try:
            for item in os.listdir(path):
                subpath = os.path.join(path, item)
                if os.path.isdir(subpath):
                    self.scan_directory(subpath, keywords, extensions, max_size)
        except:
            pass

    def perform_scan(self, search_text):
        keywords = [k.strip().lower() for k in search_text.split(',') if k.strip()]
        extensions = self.get_extensions()
        max_size = self.get_max_size()

        self.log(f"Mode: {'Filename' if self.chk_scan_by_filename.get() else 'Content'} Search", 'cyan')
        self.log(f"File types: {', '.join(extensions)}", 'cyan')
        self.log(f"Max size: {max_size//1024} KB", 'cyan')

        if self.chk_scan_all.get():
            drives = [f"{d}:\\" for d in string.ascii_uppercase if os.path.exists(f"{d}:\\")]
            for drive in drives:
                self.log(f"Scanning drive: {drive}", 'cyan')
                self.scan_directory(drive, keywords, extensions, max_size)
        else:
            path = self.txt_scan_path.get().strip()
            if path:
                self.log(f"Scanning folder: {path}", 'cyan')
                self.scan_directory(path, keywords, extensions, max_size)

    def update_status(self):
        self.lbl_status.config(text=f"Processed: {self.processed_files} | Found: {self.copied_files}")

    def start_scan(self):
        if not self.output_folder:
            messagebox.showwarning("Warning", "Please select an output folder")
            return
        search_text = self.txt_search.get("1.0", tk.END).strip()
        if not search_text:
            messagebox.showwarning("Warning", "Please enter search terms")
            return

        self.is_scanning = True
        self.processed_files = 0
        self.copied_files = 0
        self.log_box.delete(1.0, tk.END)
        self.log("🚀 Starting scan...", 'yellow')
        self.progress.start()

        thread = threading.Thread(target=self.scan_worker, args=(search_text,), daemon=True)
        thread.start()

    def scan_worker(self, search_text):
        try:
            self.perform_scan(search_text)
        finally:
            self.root.after(0, self.finish_scan)

    def finish_scan(self):
        self.is_scanning = False
        self.progress.stop()
        self.log("✅ Scan completed!", 'lime')
        self.lbl_status.config(text="Ready")

    def stop_scan(self):
        self.log("🛑 Stop requested (full stop may require restart)", 'orange')

    def clear_log(self):
        self.log_box.delete(1.0, tk.END)

if __name__ == "__main__":
    root = tk.Tk()
    app = FileScannerApp(root)
    root.mainloop()