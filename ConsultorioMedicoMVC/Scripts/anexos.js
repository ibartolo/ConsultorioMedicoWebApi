(function () {
    'use strict';

    var $modal = null;
    var urls = {};
    var consultas = [];
    var anexos = [];
    var filtroConsultaId = '';
    var cargando = false;

    function pad2(n) { return n < 10 ? '0' + n : n; }

    function formatFechaHoraAmPm(iso) {
        if (!iso) return '';
        var d;
        if (typeof iso === 'string' && iso.indexOf('/Date(') === 0) {
            d = new Date(parseInt(iso.substr(6), 10));
        } else {
            d = new Date(iso);
        }
        if (isNaN(d.getTime())) return '';
        var hh = d.getHours();
        var ampm = hh >= 12 ? 'pm' : 'am';
        var h12 = hh % 12; if (h12 === 0) h12 = 12;
        return pad2(d.getDate()) + '/' + pad2(d.getMonth() + 1) + '/' + d.getFullYear()
             + ' ' + pad2(h12) + ':' + pad2(d.getMinutes()) + ' ' + ampm;
    }

    function escapar(texto) {
        if (texto === null || texto === undefined) return '';
        return String(texto)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function truncar(texto, max) {
        if (!texto) return '';
        texto = String(texto);
        return texto.length > max ? texto.substring(0, max) + '...' : texto;
    }

    function etiquetaConsultaTexto(c) {
        var fecha = formatFechaHoraAmPm(c.FechaConsulta);
        var tipo = c.TipoConsultaNombre ? c.TipoConsultaNombre : 'Consulta';
        var desc = c.Descripcion ? ' - ' + truncar(c.Descripcion, 60) : '';
        return fecha + ' - ' + tipo + desc;
    }

    function etiquetaConsulta(id) {
        if (!id) return '<span class="text-muted">Paciente</span>';
        for (var i = 0; i < consultas.length; i++) {
            if (String(consultas[i].Id) === String(id)) return escapar(etiquetaConsultaTexto(consultas[i]));
        }
        return 'Consulta #' + escapar(id);
    }

    function llenarSelectConsultas(seleccion) {
        var $sel = $('#anexoConsultaSelect');
        var $filtro = $('#anexoFiltroConsulta');
        $sel.find('option:not(:first)').remove();
        $filtro.find('option:not(:first)').remove();

        for (var i = 0; i < consultas.length; i++) {
            var c = consultas[i];
            var texto = etiquetaConsultaTexto(c);
            $sel.append($('<option>').val(c.Id).text(texto));
            $filtro.append($('<option>').val(c.Id).text(texto));
        }

        if (seleccion) {
            $sel.val(String(seleccion));
            $filtro.val(String(seleccion));
            filtroConsultaId = String(seleccion);
        } else {
            $sel.val('');
            $filtro.val('');
            filtroConsultaId = '';
        }
    }

    function cargarConsultas(pacienteId, seleccion, callback) {
        $.get(urls.consultas, { pacienteId: pacienteId }, function (response) {
            var r = (typeof response === 'string') ? JSON.parse(response) : response;
            consultas = (r && r.IsSuccess && r.Response) ? r.Response : [];
            llenarSelectConsultas(seleccion);
            if (callback) callback();
        }).fail(function () {
            consultas = [];
            llenarSelectConsultas(seleccion);
            if (callback) callback();
        });
    }

    function cargarAnexos(pacienteId) {
        $.get(urls.listar, { pacienteId: pacienteId }, function (response) {
            var r = (typeof response === 'string') ? JSON.parse(response) : response;
            if (r && r.IsSuccess) {
                anexos = r.Response || [];
            } else {
                anexos = [];
                Swal.fire({ title: 'Error', text: (r && r.Message) || 'No fue posible cargar los anexos.', icon: 'error', confirmButtonColor: '#0099CC' });
            }
            renderAnexos();
        }).fail(function () {
            anexos = [];
            renderAnexos();
            Swal.fire({ title: 'Error', text: 'Error de conexión al cargar los anexos.', icon: 'error', confirmButtonColor: '#0099CC' });
        });
    }

    function renderAnexos() {
        var $tbody = $('#anexosModalTable tbody');
        $tbody.empty();

        var lista = anexos;
        if (filtroConsultaId) {
            lista = $.grep(anexos, function (a) { return String(a.ConsultaID) === String(filtroConsultaId); });
        }

        if (!lista || lista.length === 0) {
            $tbody.append('<tr><td colspan="5" class="text-center text-muted py-3">No hay anexos para mostrar.</td></tr>');
            return;
        }

        for (var i = 0; i < lista.length; i++) {
            var a = lista[i];
            var nombre = escapar(a.Nombre || '(sin nombre)');
            var fecha = formatFechaHoraAmPm(a.Fecha);
            var comentarios = escapar(a.Comentarios || '');
            var consulta = etiquetaConsulta(a.ConsultaID);
            var acciones = '<button type="button" class="btn btn-sm btn-primary anexo-descargar" data-id="' + a.Id + '" title="Descargar"><i class="fas fa-download"></i></button>';
            $tbody.append(
                '<tr>' +
                '<td><i class="fas fa-file me-1"></i>' + nombre + '</td>' +
                '<td>' + fecha + '</td>' +
                '<td>' + comentarios + '</td>' +
                '<td>' + consulta + '</td>' +
                '<td>' + acciones + '</td>' +
                '</tr>'
            );
        }
    }

    function validarArchivo(file) {
        var nombre = file.name || '';
        var punto = nombre.lastIndexOf('.');
        var ext = punto >= 0 ? nombre.substring(punto).toLowerCase() : '';
        var permitidas = ['.pdf', '.png', '.jpg', '.jpeg'];

        if (permitidas.indexOf(ext) < 0) return 'Solo se permiten archivos PDF, PNG, JPG o JPEG.';
        if (nombre.length > 150) return 'El nombre del archivo no puede exceder 150 caracteres.';

        var max = ext === '.pdf' ? 5 * 1024 * 1024 : 2 * 1024 * 1024;
        if (file.size > max) {
            return ext === '.pdf'
                ? 'El PDF excede el tamaño máximo de 5 MB.'
                : 'La imagen excede el tamaño máximo de 2 MB.';
        }
        return '';
    }

    function subirAnexo() {
        if (cargando) return;

        var pacienteId = $('#anexoPacienteId').val();
        if (!pacienteId || pacienteId === '0') {
            Swal.fire({ title: 'Error', text: 'No se identificó el paciente.', icon: 'error', confirmButtonColor: '#0099CC' });
            return;
        }

        var input = $('#anexoArchivo')[0];
        var file = input && input.files ? input.files[0] : null;
        if (!file) {
            Swal.fire({ title: 'Archivo requerido', text: 'Seleccione un archivo para subir.', icon: 'warning', confirmButtonColor: '#0099CC' });
            return;
        }

        var error = validarArchivo(file);
        if (error) {
            Swal.fire({ title: 'Archivo no válido', text: error, icon: 'warning', confirmButtonColor: '#0099CC' });
            return;
        }

        var comentarios = $('#anexoComentarios').val() || '';
        if (comentarios.length > 1500) {
            Swal.fire({ title: 'Comentarios demasiado largos', text: 'Los comentarios no pueden exceder 1500 caracteres.', icon: 'warning', confirmButtonColor: '#0099CC' });
            return;
        }

        var formData = new FormData();
        formData.append('pacienteId', pacienteId);
        var consultaId = $('#anexoConsultaSelect').val();
        if (consultaId) formData.append('consultaId', consultaId);
        formData.append('comentarios', comentarios);
        formData.append('archivo', file);

        cargando = true;
        var $btn = $('#btnSubirAnexo').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> Subiendo...');

        $.ajax({
            url: urls.subir,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json'
        }).done(function (response) {
            var r = (typeof response === 'string') ? JSON.parse(response) : response;
            if (r && r.IsSuccess) {
                $('#anexoArchivo').val('');
                $('#anexoComentarios').val('');
                $('#anexoComentariosContador').text(0);
                Swal.fire({ title: '¡Anexo guardado!', text: r.Message || 'El anexo se guardó correctamente.', icon: 'success', timer: 1500, showConfirmButton: false });
                cargarAnexos(pacienteId);
            } else {
                Swal.fire({ title: 'Error', text: (r && r.Message) || 'No fue posible guardar el anexo.', icon: 'error', confirmButtonColor: '#0099CC' });
            }
        }).fail(function () {
            Swal.fire({ title: 'Error', text: 'Error de conexión al subir el anexo.', icon: 'error', confirmButtonColor: '#0099CC' });
        }).always(function () {
            cargando = false;
            $btn.prop('disabled', false).html('<i class="fas fa-upload me-1"></i> Subir anexo');
        });
    }

    function descargarAnexo(id) {
        if (!id) return;
        window.open(urls.descargar + '/' + id, '_blank');
    }

    window.abrirModalAnexos = function (pacienteId, pacienteNombre, consultaId) {
        if (!$modal || !$modal.length) return;

        pacienteId = pacienteId || 0;
        consultaId = consultaId || '';

        $('#anexoPacienteId').val(pacienteId);
        $('#anexosPacienteNombre').text(pacienteNombre ? '- ' + pacienteNombre : '');
        $('#anexoArchivo').val('');
        $('#anexoComentarios').val('');
        $('#anexoComentariosContador').text(0);
        $('#anexoConsultaId').val(consultaId);

        cargarConsultas(pacienteId, consultaId, function () {
            cargarAnexos(pacienteId);
            $modal.modal('show');
        });
    };

    function init() {
        $modal = $('#modalAnexos');
        if (!$modal.length) return;

        urls.listar = $modal.data('url-listar');
        urls.subir = $modal.data('url-subir');
        urls.descargar = $modal.data('url-descargar');
        urls.consultas = $modal.data('url-consultas');

        $('#btnSubirAnexo').on('click', subirAnexo);

        $('#anexoFiltroConsulta').on('change', function () {
            filtroConsultaId = $(this).val() || '';
            renderAnexos();
        });

        $('#anexosModalTable').on('click', '.anexo-descargar', function () {
            descargarAnexo($(this).data('id'));
        });

        $('#anexoArchivo').on('change', function () {
            var file = this.files && this.files[0];
            if (!file) return;
            var error = validarArchivo(file);
            if (error) {
                Swal.fire({ title: 'Archivo no válido', text: error, icon: 'warning', confirmButtonColor: '#0099CC' });
                $(this).val('');
            }
        });

        $('#anexoComentarios').on('input', function () {
            $('#anexoComentariosContador').text(this.value.length);
        });
    }

    $(document).ready(init);
})();
