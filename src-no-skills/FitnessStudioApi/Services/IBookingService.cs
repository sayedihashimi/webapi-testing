using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IBookingService
{
    Task<BookingDto> GetByIdAsync(int id);
    Task<BookingDto> CreateAsync(CreateBookingDto dto);
    Task<BookingDto> CancelAsync(int id, CancelBookingDto dto);
    Task<BookingDto> CheckInAsync(int id);
    Task<BookingDto> MarkNoShowAsync(int id);
}
