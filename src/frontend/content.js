// Tooltip elementini oluştur
const tooltip = document.createElement("div");
tooltip.id = "pg-tooltip";
document.body.appendChild(tooltip);

let currentTarget = null; 
let isHoveringTooltip = false; 

// Linke Gelince Butonu Göster
document.addEventListener("mouseover", (e) => {
    const target = e.target.closest("a");
    
    // Eğer bir linkse ve zaten o linkin tooltip'i açık değilse
    if (target && target.href) {
        currentTarget = target;
        
        // Tooltip daha önce analiz edilip sonuç göstermiyorsa butona çevir
        showAnalyzeButton(target.href, e.pageX, e.pageY);
    }
});


//  Linkten çıkınca gizle (Ama Tooltip'e gidiyorsa gizleme)
document.addEventListener("mouseout", (e) => {
    const target = e.target.closest("a");
    if (target) {
        // kullanıcı tooltipe gidiyorsa biraz bekle
        setTimeout(() => {
            if (!isHoveringTooltip) {
                tooltip.style.display = "none";
            }
        }, 100); 
    }
});

// Tooltip üzerine gelince kapanmasın
tooltip.addEventListener("mouseenter", () => {
    isHoveringTooltip = true;
});

// Tooltip üzerinden çıkınca kapansın
tooltip.addEventListener("mouseleave", () => {
    isHoveringTooltip = false;
    tooltip.style.display = "none";
});

// 3. FONKSİYONLAR

function showAnalyzeButton(url, x, y) {
    // Tooltip pozisyonu
    tooltip.style.top = (y + 15) + "px";
    tooltip.style.left = (x + 15) + "px";
    tooltip.style.display = "block";

    // Tasarımı "Buton" moduna al
    tooltip.className = "pg-button-mode";
    tooltip.innerHTML = `Analiz Et`;

    tooltip.onclick = function(e) {
        // Linke tıklamayı engelle (Sadece butona tıklansın)
        e.preventDefault(); 
        e.stopPropagation();

        // Taramayı Başlat
        startScanning(url);
    };
}

async function startScanning(url) {
    // Görüntüyü "Taranıyor" moduna al
    tooltip.className = ""; 
    tooltip.style.backgroundColor = "white";
    tooltip.style.color = "black";
    tooltip.style.cursor = "default";
    tooltip.innerHTML = `⏳ Taranıyor...`;

    // API İsteği At
    try {
        const apiUrl = "https://localhost:7139/api/Scan"; 
        
        const response = await fetch(apiUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ 
                url: url,
                scanType: "Fast" 
            })
        });

        if (!response.ok) throw new Error("Sunucu Hatası");
        const data = await response.json();

        // Sonucu Göster
        showResult(data);

    } catch (error) {
        console.error("Hata:", error);
        tooltip.innerHTML = `<span style="color:red">⚠️ Bağlantı Hatası</span>`;
    }
}

function showResult(data) {
    const isSafe = data.riskLevel === 0 || data.riskLevel === 1; 
    
    tooltip.className = isSafe ? "pg-safe" : "pg-danger";
    
    const color = isSafe ? "#27ae60" : "#c0392b";
    const icon = isSafe ? "✅" : "⛔";
    const statusText = isSafe ? "GÜVENLİ" : "RİSKLİ";

    tooltip.innerHTML = `
        <span class="pg-header" style="color:${color}">
            ${icon} ${statusText}
            <span style="float:right">%${data.riskScore}</span>
        </span>
        <div style="color:#555; margin-top:4px; font-size:11px;">
            ${data.detectionDetails && data.detectionDetails.length > 0 
                ? data.detectionDetails[0].substring(0, 50) + "..." 
                : "Tehdit bulunamadı."}
        </div>
    `;
    
    // Sonuç geldikten sonra tıklama özelliğini kapat
    tooltip.onclick = null;
}