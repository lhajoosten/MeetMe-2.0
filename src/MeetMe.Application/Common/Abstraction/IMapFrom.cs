using AutoMapper;

namespace MeetMe.Application.Common.Abstraction
{
    /// <summary>
    /// Interface for mapping from a source type to the implementing type.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    public interface IMapFrom<TSource> where TSource : class
    {
        /// <summary>
        /// Maps from the source type to the implementing type.
        /// </summary>
        /// <param name="profile">The AutoMapper profile to use for mapping.</param>
        void Mapping(Profile profile)
        {
            profile.CreateMap(typeof(TSource), GetType());
        }
    }
}
