using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateAsync(BookingCreateDto dto);
    Task<BookingResponseDto?> GetByIdAsync(int id);
    Task<BookingResponseDto> CancelAsync(int id, CancelBookingDto? dto);
    Task<BookingResponseDto> CheckInAsync(int id);
    Task<BookingResponseDto> MarkNoShowAsync(int id);
}
