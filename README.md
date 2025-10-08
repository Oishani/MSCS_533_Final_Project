# GeoHeatmap (.NET MAUI) — Location Tracking Heat Map

A .NET MAUI mobile app (iOS) that **tracks the user’s location**, **persists samples to SQLite**, and **visualizes** them as a **heat map** over a MAUI Map with **discrete blue dots** for individual fixes. The UI contains a layout with a **purple “MainView”** top bar and a **bottom control card** (Samples, Last fix, and three action buttons).

---

## Features

- **Foreground location tracking** on a timer (permission: “While Using”).  
- **SQLite** persistence of `{ Latitude, Longitude, AccuracyMeters, TimestampUtc }` for each sample.  
- **Heat map** using SkiaSharp (blurred accumulation) + **blue dots** for precise sample locations.  
- **Controls:** Toggle Tracking, Refresh Heatmap, Clear Data.  
- **Readouts:** Samples (count), Last fix (UTC).  
- Runs on **iOS Simulator** (tested with **iOS 18.6**).

---

## Tech Stack / Packages

- **.NET** 8, **.NET MAUI** (Maps + Controls)  
- **SkiaSharp.Views.Maui.Controls** for canvas overlay  
- **sqlite-net-pcl** for SQLite ORM  
- **CommunityToolkit.Mvvm** for ViewModel plumbing

Suggested package versions:

```xml
<PackageReference Include="Microsoft.Maui.Controls" Version="8.0.100" />
<PackageReference Include="Microsoft.Maui.Controls.Maps" Version="8.0.100" />
<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
<PackageReference Include="SkiaSharp.Views.Maui.Controls" Version="2.88.6" />
```

---

## Project Structure

```
GeoHeatmap/
  App.xaml
  App.xaml.cs
  GeoHeatmap.csproj
  MauiProgram.cs
  Models/
    LocationSample.cs
  Services/
    ILocationService.cs
    MauiLocationService.cs
    IDataService.cs
    SqliteDataService.cs
    HeatmapService.cs
  ViewModels/
    MapViewModel.cs
  Views/
    MapPage.xaml
    MapPage.xaml.cs
  Platforms/
    iOS/Info.plist
    Android/AndroidManifest.xml
  Resources/
    ...
```

---

## Setup (macOS + VS Code)

1. **Install Xcode** from the App Store. Then:
   ```bash
   sudo xcode-select -s /Applications/Xcode.app/Contents/Developer
   sudo xcodebuild -runFirstLaunch
   ```
2. **Install .NET 8 SDK** and MAUI workloads:
   ```bash
   # You may need sudo if dotnet is system-wide
   sudo dotnet workload install maui
   sudo dotnet workload update
   ```
3. **VS Code extensions**: “C# Dev Kit”, “C#”, “.NET Install Tool”.

### iOS permissions (Info.plist)
Make sure these keys exist:
```xml
<key>CFBundleIdentifier</key>
<string>com.akshat.GeoHeatmap</string>
<key>NSLocationWhenInUseUsageDescription</key>
<string>Your location is used to generate a heat map.</string>
<key>NSLocationAlwaysAndWhenInUseUsageDescription</key>
<string>Needed to track location for the heat map.</string>
```

---

## Build & Run (iOS 18.6 Simulator)

Replace the `UDID` with your simulator’s UDID.

```bash
# from the GeoHeatmap project folder (where GeoHeatmap.csproj lives)
UDID=<Device ID>

# 1) Point Xcode and clean
sudo xcode-select -s /Applications/Xcode.app/Contents/Developer
rm -rf bin obj

# 2) Restore & build for iOS simulator
dotnet restore
dotnet build -f net8.0-ios

# 3) Boot the simulator (iOS 18.6 device)
xcrun simctl bootstatus "$UDID" -b || xcrun simctl boot "$UDID"

# 4) Install & launch the app
xcrun simctl install "$UDID" bin/Debug/net8.0-ios/iossimulator-arm64/GeoHeatmap.app
xcrun simctl launch  "$UDID" com.akshat.GeoHeatmap
```
---

## Using the App

1. In Simulator: **Features → Location → City Bicycle Ride** (or **Freeway Drive**).  
2. Tap **Toggle Tracking** → grant “While Using” permission if prompted.  
3. Let it run for a bit (default: one fix every 5 s).  
4. Tap **Refresh Heatmap** to paint new samples.  
5. Tap **Clear Data** to wipe the SQLite table (fresh start).

### What the buttons & readouts mean
- **Toggle Tracking** - Start/stop foreground sampling loop.  
- **Refresh Heatmap** - Repaints overlay based on all samples in SQLite.  
- **Clear Data** - Deletes all samples from the database.  
- **Samples** - Total number of saved fixes.  
- **Last fix (UTC)** - Timestamp of the most recent fix (UTC).

---

## How it works (high level)

- **Tracking loop** (`MapViewModel.TrackLoopAsync`) calls `Geolocation.Default.GetLocationAsync()` on a timer and saves each fix via `SqliteDataService.InsertAsync(...)` into `geo.db3` (under `FileSystem.AppDataDirectory`).  
- **Rendering** (`HeatmapService.DrawHeat`) draws a Gaussian-blur “heat” layer and **blue dots** for each visible sample onto an `SKCanvasView` stacked above the `Map`.  
- **UI Layout** (`MapPage.xaml`) shows the map + overlay and a bottom control card; `App.xaml` styles the top bar purple with title **“MainView”**.

### Data model
```csharp
class LocationSample {
  int Id; double Latitude; double Longitude;
  double? AccuracyMeters; DateTime TimestampUtc;
}
```

---

## Configuration & Tuning

- **Sampling interval:** in `MapViewModel.TrackLoopAsync`, change  
  ```csharp
  await Task.Delay(TimeSpan.FromSeconds(5), ct); // e.g., set to 15
  ```
- **Heat visual:** in `HeatmapService.DrawHeat(...)`, tweak `radiusPx`, `blurPx`, and alpha in the red heat paint / blue dot radius.
- **Initial zoom:** in `MapPage.xaml.cs → CenterOnUserAsync()` adjust  
  ```csharp
  MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(5))
  ```