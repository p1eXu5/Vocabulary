var mudDrawerResizeFactory = function (dotNetRef) {
    var _dotNetRef = dotNetRef;
    var _moveHandler = null;
    var _upHandler = null;

    return {
        register: function() {
            _moveHandler = (e) => {
                _dotNetRef.invokeMethodAsync('OnMouseMove', e.clientX);
            };
            _upHandler = (e) => {
                _dotNetRef.invokeMethodAsync('OnMouseUp');
                document.removeEventListener('mousemove', this._moveHandler);
                document.removeEventListener('mouseup', this._upHandler);
            };
            document.addEventListener('mousemove', this._moveHandler);
            documet.addEventListener('mouseup', this._upHandler);
        },
        unregister: function() {
            documet.removeEventListener('mousemove', this._moveHandler);
            documet.removeEventListener('mouseup', this._upHandler);
            _dotNetRef = null;
        }
    }
};

export function register(dotNetRef) {
    // TODO: return object to set to component JSObjectReference
    mudDrawerResize.register(dotNetRef);
}

export function unregister() {
    mudDrawerResize.unregister();
}