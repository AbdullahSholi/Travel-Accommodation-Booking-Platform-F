namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
        public Task<T?> GetByIdAsync(int id);
        public Task<List<T>> GetAllAsync();
        public Task AddAsync(T entity);
        public Task UpdateAsync(T entity);
        public Task DeleteAsync(T entity);
}