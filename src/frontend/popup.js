const API_URL = "https://localhost:7139/api/Scan";

document.addEventListener('DOMContentLoaded', async () => {
    // 1. aktif sekmenin urlsini al
    let [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
    
    if (tab && tab.url) {
        document.getElementById('currentUrl').textContent = tab.url;
        
        // butona tıklanınca analizi başlat
        document.getElementById('scanBtn').addEventListener('click', () => {
            startScan(tab.url);
        });
    } else {
        document.getElementById('currentUrl').textContent = "URL Alınamadı";
    }
    
    // API bağlantı testi
    checkApiStatus();
});

async function startScan(url) {
    // UI Güncelle: Butonu gizle, loaderı göster
    document.getElementById('scanBtn').style.display = 'none';
    document.getElementById('loader').classList.remove('hidden');
    document.getElementById('resultArea').classList.add('hidden');

    try {
        // 3. Backend'e İstek At (POST)
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                url: url,
                scanType: "Fast" // DTO'daki ScanType
            })
        });

        if (!response.ok) throw new Error('API Hatası');

        const result = await response.json();
        showResult(result);

    } catch (error) {
        console.error("Hata:", error);
        alert("API'ye bağlanılamadı! Lütfen Backend'in çalıştığından ve SSL sertifikasının onaylandığından emin olun.");
        document.getElementById('scanBtn').style.display = 'block';
        document.getElementById('loader').classList.add('hidden');
    }
}

function showResult(data) {
    // Loader gizle, sonucu göster
    document.getElementById('loader').classList.add('hidden');
    const resultBox = document.getElementById('resultArea');
    resultBox.classList.remove('hidden');

    // Elementleri seç
    const icon = document.getElementById('statusIcon');
    const title = document.getElementById('statusTitle');
    const scoreFill = document.getElementById('scoreFill');
    const scoreText = document.getElementById('scoreText');
    const list = document.getElementById('detailsList');
    const source = document.getElementById('sourceText');

    // Verileri Doldur
    scoreText.textContent = `Risk Skoru: %${data.riskScore}`;
    scoreFill.style.width = `${data.riskScore}%`;
    source.textContent = `Tespit Kaynağı: ${data.detectionSource}`;
    
    // Liste temizle
    list.innerHTML = '';
    data.detectionDetails.forEach(detail => {
        let li = document.createElement('li');
        li.textContent = detail;
        list.appendChild(li);
    });

    // Renk ve İkon Ayarı
    // Backend'den gelen RiskLevel Enum değerine göre (0=Safe, 4=Malicious)
    if (data.isPhishing || data.riskLevel >= 2) {
        // ZARARLI
        icon.textContent = "🚫";
        title.textContent = "TEHDİT ALGILANDI!";
        title.className = "risk-color";
        scoreFill.style.backgroundColor = "#e74c3c";
    } else if (data.riskLevel === 1) {
        // ŞÜPHELİ
        icon.textContent = "⚠️";
        title.textContent = "Dikkatli Olun";
        title.className = "warn-color";
        scoreFill.style.backgroundColor = "#f39c12";
    } else {
        // GÜVENLİ
        icon.textContent = "✅";
        title.textContent = "Güvenli Site";
        title.className = "safe-color";
        scoreFill.style.backgroundColor = "#27ae60";
    }
}

async function checkApiStatus() {
    try {
        await fetch(API_URL.replace("/Scan", "/weatherforecast")); // Basit bir GET ile kontrol
        document.getElementById('apiStatus').textContent = "🟢 Sistem Çevrimiçi";
        document.getElementById('apiStatus').style.color = "green";
    } catch {
        document.getElementById('apiStatus').textContent = "🔴 API Bağlantısı Yok";
        document.getElementById('apiStatus').style.color = "red";
    }
}