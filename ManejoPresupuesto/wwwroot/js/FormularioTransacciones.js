function inicializarFormularioTransacciones(urlObtenerCategorias) {
    // Detectar cambios en el select de tipoOperacionId
    $("#tipoOperacionId").change(async function () {
        // Obtener el valor seleccionado
        const valorSeleccionado = $(this).val();
        // Hacer la petición para obtener las categorias
        const respuesta = await fetch(urlObtenerCategorias, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: valorSeleccionado
        });
        // Procesar la respuesta
        const json = await respuesta.json();
        // Generar las opciones del select de categorias
        const opciones = json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`);
        // Actualizar el select de categorias
        $("#CategoriaId").html(opciones);
    })
}