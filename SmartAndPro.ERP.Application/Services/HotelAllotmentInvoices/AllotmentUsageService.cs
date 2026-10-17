using AutoMapper;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices
{

    public class AllotmentUsageService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly ServiceBaseFactory _serviceBaseFactory;

        public AllotmentUsageService(
            RepositoriesFactory repositoriesFactory,
            AppSettings appSettings,
            IMapper mapper,
            ServiceBaseFactory serviceBaseFactory
           )
        {
            _repositoriesFactory = repositoriesFactory;
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
        }

        public async Task<List<DailyAvailability>> GetDailyAvailability(
    int allotmentRoomId,
    DateTime from,
    DateTime to)
        {
            var roomRepo = _repositoriesFactory.GetRepository<HotelAllotmentRoom>();
            var usageRepo = _repositoriesFactory.GetRepository<AllotmentUsage>();

            var room = (await roomRepo.Filter(r => r.Id == allotmentRoomId)).Data?.FirstOrDefault()
                       ?? throw new Exception("Allotment room not found");

            var usages = (await usageRepo.Filter(u =>
                u.HotelAllotmentRoomId == allotmentRoomId &&
                u.UsageDate >= from.Date &&
                u.UsageDate < to.Date)).Data?.ToList() ?? new List<AllotmentUsage>();

            var result = new List<DailyAvailability>();

            for (var date = from.Date; date < to.Date; date = date.AddDays(1))
            {
                int totalRoomsForDay = room.RoomsPerDay.TryGetValue(date.DayOfWeek, out var r) ? r : 0;

                // نحسب مجموع الـ ConsumedRooms الموجود مسبقًا للـ day
                int currentConsumed = usages.Where(u => u.UsageDate == date)
                                            .Sum(u => u.ConsumedRooms + u.OverbookedRooms);

                int overbooked = Math.Max(0, currentConsumed - totalRoomsForDay);
                int normalConsumed = Math.Min(currentConsumed, totalRoomsForDay);

                // تحديث أو إضافة Usage
                var usage = usages.FirstOrDefault(u => u.UsageDate == date);
                if (usage != null)
                {
                    usage.ConsumedRooms = normalConsumed;
                    usage.OverbookedRooms = overbooked;
                    await usageRepo.Update(usage);
                }
                else
                {
                    await usageRepo.Add(new AllotmentUsage
                    {
                        HotelAllotmentRoomId = allotmentRoomId,
                        UsageDate = date,
                        ConsumedRooms = normalConsumed,
                        OverbookedRooms = overbooked
                    });
                }

                result.Add(new DailyAvailability
                {
                    Date = date,
                    TotalRooms = totalRoomsForDay,
                    RoomsBooked = normalConsumed + overbooked,
                    RoomsAvailable = totalRoomsForDay - normalConsumed,
                    Overbooking = overbooked > 0,
                    OverbookedRooms = overbooked
                });
            }

            return result;
        }


        //public async Task<List<DailyAvailability>> GetDailyAvailability(
        //        int allotmentRoomId,
        //        DateTime from,
        //        DateTime to
        //    )
        //    {
        //        var roomRepo = _repositoriesFactory.GetRepository<HotelAllotmentRoom>();
        //        var usageRepo = _repositoriesFactory.GetRepository<AllotmentUsage>();

        //        // 1️⃣ جلب غرفة الالوتمنت
        //        var roomResponse = await roomRepo.Filter(r => r.Id == allotmentRoomId);
        //        var room = roomResponse.Data?.FirstOrDefault()
        //            ?? throw new Exception("Allotment room not found");

        //        // 2️⃣ جلب كل ال usages للفترة
        //        var usageResponse = await usageRepo.Filter(u =>
        //                u.HotelAllotmentRoomId == allotmentRoomId &&
        //                u.UsageDate >= from.Date &&
        //                u.UsageDate < to.Date
        //            );

        //        var usages = usageResponse.Data?.ToList() ?? new List<AllotmentUsage>();


        //        var result = new List<DailyAvailability>();

        //        // 3️⃣ حساب يوم بيوم
        //        for (var date = from.Date; date < to.Date; date = date.AddDays(1))
        //        {
        //            int totalRoomsForDay =
        //                room.RoomsPerDay.TryGetValue(date.DayOfWeek, out var r)
        //                    ? r
        //                    : 0;

        //            // نحسب مجموع الـ ConsumedRooms الموجود مسبقًا للـ day
        //            int currentConsumed = usages.Where(u => u.UsageDate == date)
        //                                        .Sum(u => u.ConsumedRooms + u.OverbookedRooms);

        //            //// مجموع الغرف اللي اتحجزت للـ day
        //            //int consumedRooms = usages
        //            //    .Where(u => u.UsageDate == date)
        //            //    .Sum(u => u.ConsumedRooms);



        //            int overbookedRooms = Math.Max(0, currentConsumed - totalRoomsForDay);
        //            //int normalConsumed = consumedRooms - overbookedRooms;
        //            int normalConsumed = Math.Min(currentConsumed, totalRoomsForDay);


        //            // لو فيه overbooking، نحدث الـ usage record
        //            if (overbookedRooms > 0)
        //            {
        //                var usageToUpdate = usages.FirstOrDefault(u => u.UsageDate == date);
        //                if (usageToUpdate != null)
        //                {
        //                    usageToUpdate.ConsumedRooms = normalConsumed;
        //                    usageToUpdate.OverbookedRooms = overbookedRooms;
        //                    await usageRepo.Update(usageToUpdate);
        //                }
        //                else
        //                {
        //                    await usageRepo.Add(new AllotmentUsage
        //                    {
        //                        HotelAllotmentRoomId = allotmentRoomId,
        //                        UsageDate = date,
        //                        ConsumedRooms = normalConsumed,
        //                        OverbookedRooms = overbookedRooms
        //                    });
        //                }
        //            }


        //            //int bookedRooms = usages
        //            //    .Where(u => u.UsageDate == date)
        //            //    .Sum(u => u.ConsumedRooms);

        //            //// ======= تعديل هنا لتخزين الأوفر بوكينج =======
        //            //if (bookedRooms > totalRoomsForDay)
        //            //{
        //            //    // احسب عدد الغرف اللي متجاوزة
        //            //    int overbookedCount = bookedRooms - totalRoomsForDay;

        //            //    // لو عايز تخزنها في الـ AllotmentUsage
        //            //    var overbookedUsage = usages.FirstOrDefault(u => u.UsageDate == date);
        //            //    if (overbookedUsage != null)
        //            //    {
        //            //        // ممكن تضيف property جديدة في AllotmentUsage اسمها مثلا OverbookedRooms
        //            //        overbookedUsage.OverbookedRooms = overbookedCount;
        //            //        await usageRepo.Update(overbookedUsage);
        //            //    }
        //            //    else
        //            //    {
        //            //        // لو مفيش Usage موجود، ممكن تضيف سجل جديد
        //            //        var newUsage = new AllotmentUsage
        //            //        {
        //            //            HotelAllotmentRoomId = allotmentRoomId,
        //            //            UsageDate = date,
        //            //            ConsumedRooms = bookedRooms,
        //            //            OverbookedRooms = overbookedCount
        //            //        };
        //            //        await usageRepo.Add(newUsage);
        //            //    }
        //            //}

        //            //result.Add(new DailyAvailability
        //            //{
        //            //    Date = date,
        //            //    TotalRooms = totalRoomsForDay,
        //            //    RoomsBooked = bookedRooms,
        //            //    RoomsAvailable = totalRoomsForDay - bookedRooms,
        //            //    Overbooking= bookedRooms > totalRoomsForDay
        //            //});

        //            // لو فيه overbooking، نحدث الـ usage record
        //            if (overbookedRooms > 0)
        //            {
        //                var usageToUpdate = usages.FirstOrDefault(u => u.UsageDate == date);
        //                if (usageToUpdate != null)
        //                {
        //                    usageToUpdate.ConsumedRooms = normalConsumed;
        //                    usageToUpdate.OverbookedRooms = overbookedRooms;
        //                    await usageRepo.Update(usageToUpdate);
        //                }
        //                else
        //                {
        //                    await usageRepo.Add(new AllotmentUsage
        //                    {
        //                        HotelAllotmentRoomId = allotmentRoomId,
        //                        UsageDate = date,
        //                        ConsumedRooms = normalConsumed,
        //                        OverbookedRooms = overbookedRooms
        //                    });
        //                }
        //            }
        //        }

        //        return result;
        //    }



        public async Task<BookingCheckResult> CheckBookingRequest(int roomId, DateTime from, DateTime to, int requestedRooms)
        {
            var dailyAvailability = await GetDailyAvailability(roomId, from, to);

            var overbookedDays = dailyAvailability
                .Where(d => d.RoomsAvailable < requestedRooms)
                .ToList();

            return new BookingCheckResult
            {
                CanBook = !overbookedDays.Any(),
                OverbookedDays = overbookedDays
            };
        }


        public async Task<List<RoomDailyAvailability>> GetHotelAvailability(
       Guid hotelId,
       DateTime from,
       DateTime to)
        {
            var roomRepo = _repositoriesFactory.GetRepository<HotelAllotmentRoom>();
            var usageRepo = _repositoriesFactory.GetRepository<AllotmentUsage>();
            var roomTypeRepo = _repositoriesFactory.GetRepository<RoomType>();

            // 1️⃣ جلب كل غرف الفندق
            var roomsResponse = await roomRepo.Filter(
                r => r.HotelAllotmentInvoice.HotelId == hotelId
            );

            var rooms = roomsResponse.Data?.ToList()
                ?? new List<HotelAllotmentRoom>();

            if (!rooms.Any())
                return new List<RoomDailyAvailability>();

            var roomIds = rooms.Select(r => r.Id).ToList();

            // 2️⃣ جلب كل usages للفترة مرة واحدة
            var usagesResponse = await usageRepo.Filter(u =>
                roomIds.Contains(u.HotelAllotmentRoomId) &&
                u.UsageDate >= from.Date &&
                u.UsageDate < to.Date
            );

            var usages = usagesResponse.Data?.ToList()
                ?? new List<AllotmentUsage>();

            // 3️⃣ Group usages (RoomId + Date)
            var usageLookup = usages
                .GroupBy(u => new { u.HotelAllotmentRoomId, u.UsageDate })
                .ToDictionary(
                    g => (g.Key.HotelAllotmentRoomId, g.Key.UsageDate),
                    g => g.Sum(x => x.ConsumedRooms)
                );

            // 4️⃣ جلب RoomTypes مرة واحدة
            var roomTypesResponse = await roomTypeRepo.Filter(_ => true);

            var roomTypes = roomTypesResponse.Data?
                .ToDictionary(rt => rt.Id, rt => rt.ArabicName)
                ?? new Dictionary<Guid, string>();

            var result = new List<RoomDailyAvailability>();

            // 5️⃣ الحساب يوم بيوم
            foreach (var room in rooms)
            {
                var dailyList = new List<DailyAvailability>();

                for (var date = from.Date; date < to.Date; date = date.AddDays(1))
                {
                    int totalRoomsForDay =
                        room.RoomsPerDay.TryGetValue(date.DayOfWeek, out var r)
                            ? r
                            : 0;

                    usageLookup.TryGetValue(
                        (room.Id, date),
                        out var bookedRooms
                    );

                    dailyList.Add(new DailyAvailability
                    {
                        Date = date,
                        TotalRooms = totalRoomsForDay,
                        RoomsBooked = bookedRooms,
                        RoomsAvailable = totalRoomsForDay - bookedRooms,
                        Overbooking = bookedRooms > totalRoomsForDay
                    });
                }

                result.Add(new RoomDailyAvailability
                {
                    RoomId = room.Id,
                    RoomTypeName = room.RoomTypeId.HasValue && roomTypes.TryGetValue(room.RoomTypeId.Value, out var name)
           ? name
           : null,
                    DailyAvailability = dailyList
                });

            }

            return result;
        }


        public async Task<List<RoomDailyAvailability>> GetAllHotelRooms(Guid hotelId)
        {
            var roomRepo = _repositoriesFactory.GetRepository<HotelAllotmentRoom>();
            var roomTypeRepo = _repositoriesFactory.GetRepository<RoomType>();

            var roomsResponse = await roomRepo.Filter(r => r.HotelAllotmentInvoice.HotelId == hotelId);
            var rooms = roomsResponse.Data?.ToList() ?? new List<HotelAllotmentRoom>();

            var roomTypesResponse = await roomTypeRepo.Filter(_ => true);
            var roomTypes = roomTypesResponse.Data?
                .ToDictionary(rt => rt.Id, rt => rt.ArabicName) ?? new Dictionary<Guid, string>();

            var result = new List<RoomDailyAvailability>();

            foreach (var room in rooms)
            {
                result.Add(new RoomDailyAvailability
                {
                    RoomId = room.Id,
                    RoomTypeName = room.RoomTypeId.HasValue && roomTypes.TryGetValue(room.RoomTypeId.Value, out var name)
                        ? name
                        : null,
                    DailyAvailability = new List<DailyAvailability>() // ممكن تسيبها فاضية لو مش محتاج التفاصيل اليومية
                });
            }

            return result;
        }

        public class RoomDailyAvailability
        {
            public int RoomId { get; set; }
            public string? RoomTypeName { get; set; }
            public List<DailyAvailability> DailyAvailability { get; set; } = new();
        }


        public class DailyAvailability
        {
            public DateTime Date { get; set; }

            // إجمالي الغرف المتاحة في الالوتمنت في اليوم ده
            public int TotalRooms { get; set; }

            // الغرف اللي اتحجزت
            public int RoomsBooked { get; set; }

            // الغرف المتبقية
            public int RoomsAvailable { get; set; }

            public bool Overbooking { get; set; }

            public int OverbookedRooms { get; set; } // العدد الزائد عن الحد

        }

        public class BookingCheckResult
        {
            public bool CanBook { get; set; }
            public List<DailyAvailability> OverbookedDays { get; set; } = new();
        }
    }
}