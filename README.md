# GameRadar

متتبع الأسعار للألعاب والمنتجات الرقمية

## المميزات
- البحث عن أفضل وأرخص الأسعار للألعاب
- مقارنة الأسعار عبر منصات متعددة (Steam, CDKeys, G2A)
- تتبع تغيرات الأسعار بمرور الوقت
- واجهة مستخدم حديثة وسهلة الاستخدام
- أداء عالي مع دعم التخزين المؤقت
- دعم API رسمية مثل Steam API

## التقنيات المستخدمة

### Backend (.NET Core)
- ASP.NET Core Web API
- Redis للتخزين المؤقت
- Steam Web API
- مكتبات HTTP للتكامل مع خدمات خارجية

### Frontend (Next.js)
- Next.js 14
- TypeScript
- Tailwind CSS
- React Query
- Chart.js للرسوم البيانية

## التثبيت والتشغيل

### Backend
```bash
cd GameRadar.Api
dotnet run
```

### Frontend
```bash
cd gameradar-web
npm install
npm run dev
```

## المتطلبات
- .NET 9.0 SDK
- Node.js 18+
- Redis Server (للتخزين المؤقت)
- Steam API Key (اختياري للوظائف المتقدمة)

## المساهمة
نرحب بالمساهمات! يرجى قراءة ملف CONTRIBUTING.md للحصول على المزيد من المعلومات.
