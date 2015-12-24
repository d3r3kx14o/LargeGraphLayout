$(':button').click(function () {
    var form_data = new FormData($('form')[0]);
    $.ajax({
        url: '/api/data/upload', //Server script to process data
        type: 'POST',
        success: function (response) {
            console.log(response);
        },
        error: function (err) {
            console.log(err);
        },
        data: form_data,
        //Options to tell jQuery not to process data or worry about content-type.
        cache: false,
        contentType: false,
        processData: false
    });
});
var invert_map = {};
var graph_dataset = 'demo';
var svg;
var get_continuous_id_graph = function (nodes, links) {
    var invert_map = {};
    for (var i = 0; i < nodes.length; i++) {
        invert_map[nodes[i].id] = i;
    }
    return {
        nodes: nodes,
        links: links.map(function (link) {
            return {
                source: invert_map[link.source],
                target: invert_map[link.target]
            };
        }),
        map: invert_map
    };
};

var plot_graph = function(dataset, rootnodeid) {
    $.post('api/data/retrieve', {
        dataset: dataset,
        rootnodeid: rootnodeid
    }, function (response) {
        var graph = JSON.parse(response);
        var links = graph.Links;

        var floating_nodes = graph.PrimaryNodes.map(function (id, pos) {
            return {
                id: id
            }
        });
        var center = { x: width / 2, y: height / 2 };
        var side_length = Math.min(width, height) / 2;
        var border = side_length * 0.2;
        var radius = side_length - border;
        var fixed_nodes_count = graph.SecondaryNodes.length;
        var fixed_nodes = graph.SecondaryNodes.map(function (id, pos) {
            var direction = pos / fixed_nodes_count * Math.PI * 2;
            return {
                id: id,
                fixed: true,
                x: Math.cos(direction) * radius + center.x,
                y: Math.sin(direction) * radius + center.y
            }
        });
        var nodes = floating_nodes.concat(fixed_nodes);
        var new_graph = get_continuous_id_graph(nodes, links);
        draw_graph(nodes, new_graph.links);
    });
};

var force, width, height, node, link, nodes, links, labels;
var init_graph = function() {
    width = window.innerWidth - 50,
    height = window.innerHeight - 50;

    //var color = d3.scale.category10();

    nodes = [],
    links = [];

    force = d3.layout.force()
        .nodes(nodes)
        .links(links)
        .charge(-400)
        .linkDistance(120)
        .size([width, height])
        .on('tick', tick);

    var svg = d3.select('body').append('svg')
        .attr('width', width)
        .attr('height', height);

    node = svg.selectAll('.node'),
        link = svg.selectAll('.link');
    labels = node.append('text');

    plot_graph(graph_dataset, 0);
};

init_graph();

var draw_graph = function (ns, ls) {
    nodes = ns;
    links = ls;

    force
        .nodes(nodes)
        .links(links)
        .on('tick', tick);

    link = link.data(force.links(), function (d) { return d.source.id + '-' + d.target.id; });
    link.enter().insert('line', '.node').attr('class', 'link');
    link.exit().remove();

    node = node.data(force.nodes(), function (d) { return d.id; });
    node.append('g');
    var circle = node.enter().append('circle').attr('class', function (d) { return 'node ' + d.id; }).attr('r', 5)
        .on('dblclick', function (d) {
            plot_graph(graph_dataset, d.id);
        })
    ;
    var labels = node.append('svg:text').text(function(d) { return d.id; })
        .attr('dx', 12)
        .attr('dy', '.35em');
    node.exit().remove();

    force.start();
};

var tick = function() {
    node.attr('cx', function(d) { return d.x; })
        .attr('cy', function(d) { return d.y; });

    link.attr('x1', function(d) { return d.source.x; })
        .attr('y1', function(d) { return d.source.y; })
        .attr('x2', function(d) { return d.target.x; })
        .attr('y2', function (d) { return d.target.y; });

    labels
        .attr('x', function (d) {
        return d.x;
        })
        .attr('y', function (d) {
            return d.y - 10;
        }
    );
};