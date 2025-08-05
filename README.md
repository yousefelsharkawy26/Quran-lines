#                                                                      Quran Lines Service API 🕌

خدمة API شاملة للحصول على سطور القرآن الكريم مع الترجمات والتقسيم الذكي

## المميزات الجديدة ✨

### 🆕 **سطور المصحف (Mushaf Lines)**
- 📜 **تجميع الجزئيات** لتكوين سطور كما تظهر في المصحف الشريف
- 📏 **تحديد عدد السطور** في الصفحة (10-20 سطر)
- 🔗 **ربط الآيات بالسطور** مع تتبع أرقام الآيات في كل سطر
- ⚖️ **توازن السطور** بين النص العربي والترجمة

### 🌍 **ترجمات متعددة (Multi-Translation)**
- 🎯 **آية واحدة بعدة ترجمات** في طلب واحد
- 📊 **تقسيم ذكي لكل ترجمة** مع مراعاة خصائص اللغة
- ✅ **تتبع الترجمات الناجحة والفاشلة**
- 📍 **معلومات شاملة** (رقم الجزء، الحزب، الصفحة)

## المميزات الأساسية ✨

- 📖 **استخراج سطور القرآن** حسب رقم السورة والحزب والصفحة
- 🌍 **ترجمات متعددة** (الإنجليزية، الأردية، الفرنسية، الألمانية، الإسبانية، التركية، وأكثر)
- 🧠 **تقسيم ذكي للنصوص**:
  - للنص العربي: علامات الوقف القرآنية + التقسيم الدلالي
  - للترجمات الإنجليزية: النقاط الطبيعية + أدوات الربط
  - للغات الأخرى: تقسيم متوازن ومراعاة خصائص اللغة
- ✅ **تحقق من صحة البيانات** المدخلة
- 🚀 **أداء عالي** مع إدارة الأخطاء
- 📝 **توثيق شامل** مع Swagger UI

## كيفية التشغيل 🚀

### 1. استنساخ المشروع
```bash
git clone [repository-url]
cd QuranLinesService
```

### 2. تشغيل المشروع
```bash
dotnet restore
dotnet run
```

### 3. الوصول للـ API
- Swagger UI: `https://localhost:7234`
- API Base URL: `https://localhost:7234/api/quran`

## API Endpoints 📚

### 1. الحصول على سطور القرآن العادية
```
POST /api/quran/lines
```

### 🆕 2. الحصول على سطور المصحف (جديد)
```
POST /api/quran/mushaf-lines
```

**Request Body:**
```json
{
  "surahNumber": 1,
  "hizbNumber": 1,
  "pageNumber": 1,
  "translation": "english",
  "linesPerPage": 15,
  "combineSegments": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "تم استرداد سطور المصحف بنجاح",
  "mushafLines": [
    {
      "arabicLine": "بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ ٱلْحَمْدُ لِلَّهِ",
      "translationLine": "In the name of Allah, the Entirely Merciful, the Especially Merciful. Praise to Allah",
      "lineNumber": 1,
      "surahNumber": 1,
      "surahName": "Al-Fatiha",
      "pageNumber": 1,
      "hizbNumber": 1,
      "ayahNumbers": [1, 2],
      "originalSegments": ["بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ", "ٱلْحَمْدُ لِلَّهِ"],
      "translationSegments": ["In the name of Allah, the Entirely Merciful", "Praise to Allah"],
      "isCompleteLine": true
    }
  ],
  "totalLines": 5,
  "totalAyahs": 7
}
```

### 🆕 3. الحصول على آية بترجمات متعددة (جديد)
```
POST /api/quran/multi-translation
```

**Request Body:**
```json
{
  "surahNumber": 1,
  "ayahNumber": 1,
  "translations": ["english", "urdu", "french", "german"],
  "includeArabic": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "تم استرداد 4 ترجمة بنجاح",
  "ayah": {
    "arabicText": "بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ",
    "translations": {
      "english": "In the name of Allah, the Entirely Merciful, the Especially Merciful.",
      "urdu": "شروع کرتا ہوں اللہ کے نام سے جو بڑا مہربان نہایت رحم والا ہے",
      "french": "Au nom d'Allah, le Tout Miséricordieux, le Très Miséricordieux.",
      "german": "Im Namen Allahs, des Allerbarmers, des Barmherzigen."
    },
    "surahNumber": 1,
    "ayahNumber": 1,
    "surahName": "Al-Fatiha",
    "surahNameArabic": "الفاتحة",
    "juz": 1,
    "hizb": 1,
    "page": 1,
    "translationSegments": {
      "english": ["In the name of Allah,", "the Entirely Merciful, the Especially Merciful."],
      "urdu": ["شروع کرتا ہوں اللہ کے نام سے", "جو بڑا مہربان نہایت رحم والا ہے"]
    },
    "arabicSegments": ["بِسْمِ ٱللَّهِ", "ٱلرَّحْمَٰنِ ٱلرَّحِيمِ"]
  },
  "availableTranslations": ["english", "urdu", "french", "german", "spanish", "turkish", "indonesian", "russian", "chinese", "bengali"],
  "failedTranslations": []
}
```

### 4. الحصول على قائمة الترجمات المتاحة
```
GET /api/quran/translations
```

### 5. التحقق من صحة الطلبات
```
POST /api/quran/validate              # للطلبات العادية
POST /api/quran/validate-mushaf       # لطلبات المصحف
POST /api/quran/validate-multi-translation  # لطلبات الترجمات المتعددة
```

### 6. فحص حالة الخدمة
```
GET /api/quran/health
```

## الترجمات المدعومة 🌍

- `english` - الإنجليزية (Sahih International)
- `urdu` - الأردية (Jalandhry)
- `french` - الفرنسية (Hamidullah)
- `german` - الألمانية (Abu Rida)
- `spanish` - الإسبانية (Cortes)
- `turkish` - التركية (Diyanet)
- `indonesian` - الإندونيسية
- `russian` - الروسية
- `chinese` - الصينية
- `bengali` - البنغالية

## أمثلة الاستخدام الجديدة 💡

### مثال 1: الحصول على سطور المصحف
```csharp
var mushafRequest = new QuranMushafLinesRequest
{
    SurahNumber = 2,
    HizbNumber = 1,
    PageNumber = 2,
    Translation = "english",
    LinesPerPage = 15,
    CombineSegments = true
};

var response = await quranService.GetMushafLinesAsync(mushafRequest);
```

### مثال 2: الحصول على آية بترجمات متعددة
```csharp
var multiRequest = new MultiTranslationRequest
{
    SurahNumber = 2,
    AyahNumber = 255, // آية الكرسي
    Translations = new List<string> { "english", "urdu", "french" },
    IncludeArabic = true
};

var response = await quranService.GetMultiTranslationAyahAsync(multiRequest);
```

## المقارنة بين الطرق 🔄

| الميزة | السطور العادية | سطور المصحف | الترجمات المتعددة |
|--------|----------------|-------------|-------------------|
| **التقسيم** | مقسم لجزئيات | مجمع في سطور | مقسم حسب اللغة |
| **النطاق** | صفحة كاملة | صفحة مُنسقة | آية واحدة |
| **الترجمات** | واحدة فقط | واحدة فقط | متعددة |
| **الاستخدام** | التحليل التفصيلي | القراءة المتدفقة | المقارنة والدراسة |

## اختبار API 🧪

```bash
# تشغيل الاختبارات
dotnet test

# تشغيل أمثلة الاستخدام الجديدة
dotnet run --project Examples new-features

# تشغيل جميع الأمثلة
dotnet run --project Examples all
```

## Docker Support 🐳

```bash
# بناء الصورة
docker build -t quran-lines-service .

# تشغيل الحاوية
docker run -p 8080:80 quran-lines-service
```

## التحديثات الجديدة 🆕

### v2.0.0 - المميزات الجديدة
- ✅ **سطور المصحف**: تجميع الجزئيات لتكوين سطور كاملة
- ✅ **ترجمات متعددة**: الحصول على آية بعدة ترجمات
- ✅ **تحسين التقسيم**: خوارزميات أفضل للتقسيم الذكي
- ✅ **معلومات شاملة**: تفاصيل أكثر عن كل آية (الجزء، الحزب، إلخ)
- ✅ **اختبارات شاملة**: اختبارات جديدة للمميزات الجديدة

## المساهمة 🤝

نرحب بالمساهمات! يرجى إنشاء Pull Request أو فتح Issue للاقتراحات.

## الرخصة 📄

هذا المشروع مفتوح المصدر ومتاح تحت رخصة MIT.

---

### 📞 الدعم والتواصل

- **Issues**: استخدم GitHub Issues للإبلاغ عن الأخطاء
- **Features**: اقترح مميزات جديدة عبر GitHub Discussions
- **API Status**: تحقق من `/health` endpoint

**صُنع بـ ❤️ لخدمة القرآن الكريم**

