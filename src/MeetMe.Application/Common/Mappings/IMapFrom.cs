using AutoMapper;

namespace MeetMe.Application.Common.Mappings
{
    /// <summary>
    /// Interface to implement automatic mapping from a source type
    /// </summary>
    /// <typeparam name="T">The source type to map from</typeparam>
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
    }
}
