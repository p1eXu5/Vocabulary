var mudDrawerResizeFactory = (function () {
    var _dotNetRef = null;
    var _moveHandler = null;
    var _upHandler = null;
    var privateVar = 5;

    console.log("Initializing...");

    return {
        getPrivateVar: function () {
            return privateVar;
        },

        setPrivateVar: function (v) {
            privateVar = v;
        },

        register: function(dotNetRef) {
            console.log("Registering...");

            _dotNetRef = dotNetRef;

            _moveHandler = (e) => {
                _dotNetRef.invokeMethodAsync('OnMouseMove', e.clientX);
            };
            
            _upHandler = (e) => {
                _dotNetRef.invokeMethodAsync('OnMouseUp');
                document.removeEventListener('mousemove', _moveHandler);
                document.removeEventListener('mouseup', _upHandler);
            };

            document.addEventListener('mousemove', _moveHandler);
            document.addEventListener('mouseup', _upHandler);
        },
        unregister: function() {
            document.removeEventListener('mousemove', _moveHandler);
            document.removeEventListener('mouseup', _upHandler);
            _dotNetRef = null;
        }
    }
})();

export function register(_dotNetRef) {
    // TODO: return object to set to component JSObjectReference
    // mudDrawerResize.register(dotNetRef);
    mudDrawerResizeFactory.register(_dotNetRef);
    //console.log("Registering mudDrawerResize", mudDrawerResizeFactory.getPrivateVar(), _dotNetRef);
    //mudDrawerResizeFactory.setPrivateVar(10);
    //console.log("Registering mudDrawerResize", mudDrawerResizeFactory.getPrivateVar());
}

export function unregister() {
    mudDrawerResizeFactory.unregister();
}