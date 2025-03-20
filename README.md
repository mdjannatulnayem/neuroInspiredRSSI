# 🌌 A Neuromorphic Approach for Space Weather Prediction and Magnetic Reconnection using LAVA

## 📖 Introduction
This is a research-driven project exploring neuromorphic computing for space weather prediction. It integrates machine learning, traditional clustering methods, and emerging neuromorphic techniques to forecast space weather severity and detect potential **magnetic reconnection events**. By leveraging real-time satellite data, this system provides early warnings for geomagnetic disruptions.

## ❓ Why Is This Important?
Space weather events, such as **solar storms and coronal mass ejections (CMEs)**, can severely impact **power grids, GPS systems, satellites, and communication networks**. Accurately predicting these events enables:
- **Better preparedness** for geomagnetic storms.
- **Reduced impact on infrastructure** (aviation, navigation, power grids).
- **Advancement in AI-driven space weather forecasting** using neuromorphic computing.

This project explores **Intel's LAVA SDK** to optimize predictions using neuromorphic methods, aiming for improved efficiency and performance compared to traditional ML approaches.

---

## 🛠 Technology Stack
### **AI Engine**
- **Python, Jupyter Notebooks** for training models
- **Clustering (K-Means) & Regression** for space weather severity analysis
- **Flask API** for serving trained models
- **Intel LAVA SDK** (in R&D) for neuromorphic implementation

### **API System**
- **.NET 8 Backend**
- **Flask for ML inference**
- **Microsoft SQL Server** for storing geomagnetic & satellite data
- **Background worker** for real-time monitoring & warnings

### **Frontend**
- **Svelte (JavaScript Framework)**
- **Vercel for Deployment**
- **Interactive UI** for real-time space weather visualization

### **Infrastructure & Hosting**
- **Microsoft Azure** (Backend & Database)
- **Vercel** (Frontend Deployment)

---

## 🚀 How to Run
### **1️⃣ Clone the Repository**
```bash
git clone https://github.com/yourusername/neuroInspiredRSSI.git
cd neuroInspiredRSSI
```

### **2️⃣ Setup AI Engine (Model Training & Inference)**
```bash
cd AI Engine
pip install -r requirements.txt
jupyter notebook  # Run training notebooks
```

### **3️⃣ Run the Flask Inference Server**
```bash
cd AI Engine/Inference
python app.py
```

### **4️⃣ Setup and Run the API**
```bash
cd API
dotnet restore
dotnet run
```

### **5️⃣ Run the Frontend**
```bash
cd Frontend
npm install
npm run dev
```

---

## 📡 Data Sources
NeuroInspiredRSSI utilizes real-time data from **NASA & NOAA**:
- **DSCOVR Satellite** (Deep Space Climate Observatory) – Monitors solar wind & geomagnetic disturbances at **Lagrange Point 1 (L1)**.
- **ACE Satellite** (Advanced Composition Explorer) – Captures high-energy particles & solar wind conditions.
- **L2 Space Observations** – Data from Lagrange Point 2 may be integrated for extended space weather tracking.

---

## 👤 About Me
I am a **Hardware Design Engineer** by profession. Researching **neuromorphic computing** for real-time prediction systems. This project is a step toward integrating **brain-inspired AI** for better space weather forecasting.

🚀 Always looking to connect with like-minded researchers, AI developers, and space weather enthusiasts!

---

## 🤝 Collaboration
Interested in **space weather prediction, neuromorphic computing, or AI for real-time systems**? Let’s collaborate!
- Open to **contributions & research discussions**.
- Looking for **experts in neuromorphic computing (Intel Lava SDK)**.
- Contributions in **frontend, data processing, and visualization** are welcome.

📩 **Let’s build the future of AI-driven space weather forecasting together!** Reach out via GitHub Issues or pull requests.

---
🌠 *Empowering space weather forecasting with AI & neuromorphic computing!*

