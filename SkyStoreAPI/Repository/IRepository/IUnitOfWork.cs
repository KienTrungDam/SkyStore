﻿namespace SkyStoreAPI.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IUserRepository User { get; }
        IApplicationUserRepository ApplicationUser { get; }
        Task SaveAsync();
    }
}
