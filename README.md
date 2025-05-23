# Tubes1_D1753ALG
![Foto Tim](media/D1753ALG_TEAM.jpg)
## 📄 README - Dokumentasi Bot Robocode

Repositori ini bernama **Tubes1_D1753ALG** dan berisi implementasi beberapa bot dengan algoritma greedy untuk permainan Robocode Tank Royale.

### 📁 Struktur Direktori
```
Tubes1_D1753ALG
├── src/
│   ├── main-bot/
|   |   └── Jeb/        
│   └── alternative-bots/    
│       ├── B3040PKK/
│       ├── D1753ALG/
│       └── F3812FIO/
├── doc/
│   └── D1753ALG.pdf     
└── README.md                
```

---

### i. 🧠 Penjelasan Singkat Algoritma Greedy
![Foto pertarungan](media/Battle.png)

Setiap bot dalam repositori ini menggunakan **algoritma greedy**, yaitu strategi pengambilan keputusan berdasarkan pilihan terbaik pada saat itu tanpa mempertimbangkan dampak jangka panjang.

#### Bot Utama (Jeb):
Penjelasan algoritma greedy yang digunakan: Bot Jeb_ menggabungkan strategi dari semua bot, memilih posisi optimal dan adaptif dalam menyerang.


#### Bot Alternatif (B3040PKK, D1753ALG, F3812FIO):
- **B3040PKK**: Bot agresif dan fokus menyerang musuh pertama yang discan.
- **D1753ALG**: Bot menggunakan strategi pencarian musuh locking dengan penembakan prediktif yang juga menghindari musuh perdasarkan jarak musuh.
- **F3812FIO**: Bot menarget musuh berpola dan menembak dengan prediksi gerakan, menjaga jarak aman.

---

### ii. 💻 Requirement Program & Instalasi
Untuk menjalankan program ini, dibutuhkan:

- [.NET SDK 6.0+](https://dotnet.microsoft.com/en-us/download)
- [Robocode Tank Royale Bot API for C#](https://robocode-dev.github.io/tank-royale/)
- Editor kode seperti **Visual Studio**, **VS Code**, atau **JetBrains Rider**
- File konfigurasi JSON untuk masing-masing bot

---

### iii. 🛠️ Langkah Build & Compile Program
Untuk membangun dan menjalankan bot:

1. **Masuk ke direktori bot** yang ingin dijalankan, misalnya:
   ```bash
   cd src/main-bot/Jeb
   ```

2. **Build project menggunakan perintah .NET:**
   ```bash
   dotnet build
   ```

3. **Jalankan bot:**
   ```bash
   dotnet run
   ```

> Alternatif: jika tersedia, kamu juga bisa menjalankan script langsung seperti:
> ```bash
> ./Jeb.cmd
> ```

> Ulangi langkah di atas untuk masing-masing bot alternatif di folder `alternative-bots` jika ingin dijalankan atau diuji.

---

### iv. 👤 Author
**Nama: Kenneth Poenadi**  
**NIM: 13523040**  
**Email: 13523040@std.stei.itb.ac.id**  

**Nama: Bob Kunanda**  
**NIM: 13523086**  
**Email: bobkunanda@gmail.com**

**Nama: Muhammad Zahran Ramadhan Ardiana**  
**NIM: 13523104**  
**Email: 13523104@std.stei.itb.ac.id**

---



