# IKProje - HR Management System

## Proje Hakkında

IKProje, insan kaynakları yönetimi için geliştirilmiş bir web uygulamasıdır. Angular frontend ve ASP.NET Core backend ile geliştirilmiş olup, MSSQL veritabanı kullanmaktadır. Projede kullanıcı yönetimi, rol yönetimi ve izin sistemi gibi temel HR işlemleri modüler yapıda sunulmaktadır.

## Özellikler

- Kullanıcı ekleme, düzenleme ve silme
- Kullanıcı rol seçimi ve rol tabanlı erişim kontrolü
- İzin sistemi: izin talebi oluşturma, onaylama/reddetme
- Frontend ve backend tamamen modüler şekilde tasarlanmıştır
- Lazy loading ile performans optimizasyonu
- REST API entegrasyonu

## Teknolojiler

- **Frontend:** Angular, TypeScript, HTML, CSS
- **Backend:** ASP.NET Core WebAPI, C#
- **Veritabanı:** MSSQL
- **ORM:** Entity Framework Core (Code First)

## Kurulum

1. Repository’yi klonlayın:

```bash
git clone https://github.com/emirkilicerdev/IKProje.git
```

2. **Backend için:**
   - Visual Studio’da `WebAPI` projesini açın
   - `appsettings.json` dosyasından MSSQL bağlantınızı düzenleyin
   - `Update-Database` komutu ile migration’ları uygulayın
3. **Frontend için:**
   - `WebAngular` klasörüne gidin
   - Gerekli paketleri yükleyin:

```bash
npm install
```

- Angular uygulamasını çalıştırın:

```bash
ng serve
```

## Kullanım

- Uygulamayı açtıktan sonra giriş yapın
- Kullanıcı rolünü seçin
- Kullanıcı listesi ve izin işlemleri ile çalışmaya başlayabilirsiniz

## Katkıda Bulunma

Projeye katkıda bulunmak için pull request gönderebilir veya issue açabilirsiniz.

## Lisans

Bu proje MIT Lisansı ile lisanslanmıştır.

