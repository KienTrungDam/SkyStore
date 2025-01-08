﻿using SkyStoreAPI.Data;
using SkyStoreAPI.Models;
using SkyStoreAPI.Repository.IRepository;
using SkyStoreAPI.Untility;

namespace SkyStoreAPI.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public bool IsValidStatus(string currentStatus, string newStatus)
        {
            switch (currentStatus)
            {
                case SD.Order_Pending:
                    return newStatus == SD.Order_Confirmed || newStatus == SD.Order_Cancelled;
                case SD.Order_Confirmed:
                    return newStatus == SD.Order_Processing || newStatus == SD.Order_Cancelled;
                case SD.Order_Processing:
                    return newStatus == SD.Order_Completed;
                case SD.Order_Completed:
                    return false;
                case SD.Order_Cancelled:
                    return false;
                default:
                    return false;
            }
        }

        public async Task<OrderHeader> UpdateAsync(OrderHeader entity)
        {
            _db.OrderHeaders.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}