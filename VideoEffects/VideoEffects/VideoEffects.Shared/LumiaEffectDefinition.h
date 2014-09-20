#pragma once

namespace VideoEffects
{
    public ref class LumiaEffectDefinition sealed
#if WINAPI_FAMILY==WINAPI_FAMILY_PHONE_APP
        : public Windows::Media::Effects::IVideoEffectDefinition
#else
        : public VideoEffects::IVideoEffectDefinition
#endif
    {
    public:

        ///<summary>Creates an effect definition from a class implementing IFilterChainFactory.</summary>
        ///<param name='filterChainFactory'>ActivatableClassId of the filter factory.</param>
        LumiaEffectDefinition(_In_ Platform::String^ filterChainFactory);

        ///<summary>Creates an effect definition from a FilterChainFactory delegate.
        ///</summary>
        [Windows::Foundation::Metadata::DefaultOverload]
        LumiaEffectDefinition(_In_ VideoEffects::FilterChainFactory^ filterChainFactory);

        virtual property Platform::String^ ActivatableClassId 
        { 
            Platform::String^ get()
            {
                return _activatableClassId;
            }
        }
        virtual property Windows::Foundation::Collections::IPropertySet^ Properties
        { 
            Windows::Foundation::Collections::IPropertySet^ get()
            {
                return _properties;
            }
        }

    private:
        
        Platform::String^ _activatableClassId;
        Windows::Foundation::Collections::IPropertySet^ _properties;
    };
}
