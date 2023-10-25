namespace RemoteControlServer.BusinessLogic.Repository
{
    public interface IGenericRepository<T> 
        where T : class
    {
        /// <summary>
        /// Получение всех объектов
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Получение объекта по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> FindByIdAsync(int id);

        /// <summary>
        /// Добавление объекта
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true - объект добавлен, false - ошибка добавления</returns>
        Task<bool> AddAsync(T item);

        /// <summary>
        /// Изменение объекта
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true - объект изменен, false - ошибка обновления</returns>
        Task<bool> UpdateAsync(T item);

        /// <summary>
        /// Удаление объекта по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true - объект удален, false - такого объекта нет</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Сохранение изменений
        /// </summary>
        /// <returns>true - объект сохранен, false - ошибка сохранения</returns>
        Task<bool> SaveChangesAsync();

        Task<T> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    }
}
