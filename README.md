# ZDC Reference Tool

A web-based reference tool for Washington ARTCC (ZDC) controllers on VATSIM. Built on the [ZOA Info Tool](https://github.com/vzoa/info-tool-web) by Oakland ARTCC.

**Features:**
- Live D-ATIS for DCA, IAD, BWI, CLT, RDU, RIC, GSO
- IAP charts lookup
- Scratchpad references (BWI, DCA, IAD, RIC, CHO, FDK) from PCT 7110.65H
- LOA preferred routes
- ICAO airline and aircraft code lookup
- ZDC VNAS position data
- Procedure/SOP document viewer
- PIREP encoder
- Terminal command interface

---

## Prerequisites

Install the following before setting up:

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) **v18 or higher** (LTS recommended)
- [Git](https://git-scm.com/)

---

## Setup

### 1. Clone the repository

```
git clone https://github.com/RowanYoung01/ZDCinfotool-web.git
cd ZDCinfotool-web
```

### 2. Install Node dependencies

```
npm install
```

### 3. Add your PDF documents

Copy any SOP or reference PDFs into the `wwwroot/zdcpdfs/` folder.

Then register them in `appsettings.Development.json` under `CustomDocuments`:

```json
{
  "Name": "Your Document Name",
  "Url": "http://localhost:5063/zdcpdfs/your-file.pdf"
}
```

> The `wwwroot/zdcpdfs/` folder is excluded from git — PDFs are not committed to the repository.

---

## Running the Tool

Double-click **`Start-ZDCReftool.bat`** in the repository folder.

Once the console shows:

```
Now listening on: http://localhost:5063
```

Open your browser to **http://localhost:5063**

To stop the server, press **Ctrl+C** in the console window or close it.

---

## Run on Windows Startup (Recommended)

To have the tool start automatically when you log into Windows:

1. Press **Win + R**, type `shell:startup`, and press Enter
2. Right-click inside the Startup folder and select **New > Shortcut**
3. Browse to `Start-ZDCReftool.bat` in your repository folder and select it
4. Click **Finish**

The tool will now start automatically in the background every time you log in. Open your browser to **http://localhost:5063** whenever you need it.

---

## Configuration

All settings are in `appsettings.json` (production) and `appsettings.Development.json` (local overrides).

### Airports

Edit the `ArtccAirports` section in `appsettings.json` to add or remove airports:

```json
"ArtccAirports": {
  "Bravos": [ "KDCA", "KIAD", "KBWI", "KCLT", "KRDU" ],
  "Charlies": [ "KRIC", "KGSO", "KFAY" ],
  "Deltas": [ "KHEF", "KMTN", ... ],
  "Other": [ "PCT", "ZDC", ... ],
  "AtisAirports": [ "KDCA", "KIAD", "KBWI", "KCLT", "KRDU", "KRIC", "KGSO" ]
}
```

### Scratchpads

Edit `Assets/Data/v1/scratchpads.json`. Each airport entry uses its IATA/LID code (e.g. `BWI`, `DCA`):

```json
[
  {
    "id": "BWI",
    "scratchpads": [
      { "entry": "R28", "description": "Landing Runway 28" }
    ]
  }
]
```

After editing, also copy the file to `wwwroot/data/v1/scratchpads.json` so the local server picks it up, or restart the server.

### LOA Routes

Edit `Assets/Data/v1/loa.csv`. Format is:

```
Departure_Regex,Arrival_Regex,Route,RNAV Required,Notes
KDCA,KJFK,..LENDY,TRUE,Jets FL240+
```

Also copy to `wwwroot/data/v1/loa.csv` after editing.

### Documents

Add categories and PDFs in the `CustomDocuments` section of `appsettings.Development.json`:

```json
"CustomDocuments": [
  {
    "Name": "PCT SOPs",
    "Documents": [
      {
        "Name": "PCT 7110.65H CHG 1 (Oct 2024)",
        "Url": "http://localhost:5063/zdcpdfs/PCT_7110_65H_CHG_1_with_TOC.pdf"
      }
    ]
  }
]
```

---

## Troubleshooting

**`npm run css:build` failed / build error on first run** — Run the batch file as Administrator (right-click > Run as administrator). This is usually a permissions issue on first install. It only needs to happen once to install the Node packages.

**`Cannot find module 'node:path'` error during npm install** — Your Node.js version is too old. Tailwind CSS 3.3+ requires Node.js 18 or higher. Download the latest LTS version from [nodejs.org](https://nodejs.org) and reinstall.

**`.csproj` file not found** — Make sure you are running `Start-ZDCReftool.bat` from inside the repository folder, not from a shortcut pointing to a different directory. The batch file handles this automatically if double-clicked from the correct location.

**Downloaded as a ZIP and it won't run** — When extracting a GitHub ZIP, make sure you extract the *contents* of the zip so that `ZdcReference.csproj` and `Start-ZDCReftool.bat` are in the same folder. Avoid double-nested folders (e.g. `ZDCinfotool-web-main\ZDCinfotool-web-main\...`).

**Port already in use** — Another instance may already be running. Check Task Manager for `ZdcReference.exe` and end it, then restart.

**Page won't load** — Make sure the console window is still open and shows "Now listening on: http://localhost:5063". If it crashed, run the batch file again.

**Documents not showing** — Confirm the PDF is in `wwwroot/zdcpdfs/` and the URL in `appsettings.Development.json` matches the filename exactly.

**Changes to data files not appearing** — The server caches data on startup. Restart the server after editing any config or data files.

---

## Credits

Based on [info-tool-web](https://github.com/vzoa/info-tool-web) by [Oakland ARTCC (ZOA)](https://oakartcc.org). Adapted for Washington ARTCC (ZDC).
