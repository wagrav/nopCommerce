var topMenuMobile = {
    rootRoute: '',
    subCatRoute : '',
    rootselector: '',

    init: function (rootRoute, subCatRoute, rootselector) {
        this.rootRoute = rootRoute;
        this.subCatRoute = subCatRoute;
        this.rootselector = $(rootselector);

        this.catLineAttr = 'catMobId';

        this.getRoot();
    },

    getRoot: function () {
        var self = this;
        $.ajax({
            cache: false,
            url: this.rootRoute,
            type: 'post',
            success: function (result) {
                var lis = self.categoryList(result);
                $(self.rootselector).append(lis);
            },
            error: this.ajaxFailure
        });
    },

    getSubRoot: function (id) {
        var self = this;

        var catSel = 'li[' + this.catLineAttr + ' = ' + id + ']';
        if ($(catSel).hasClass("loaded")) { return };

        $.ajax({
            cache: false,
            data: {
                "id": id,
            },
            url: this.subCatRoute,
            type: 'post',
            success: function (result) {
                var listItems = self.categoryList(result);
                let ul = document.createElement('ul');
                ul.classList.add('sublist');

                $(catSel).addClass("loaded");
                $(catSel).append(ul);

                $(catSel + ' > ul').append(listItems);

                $('.top-menu.mobile .sublist-toggle').unbind().on('click', function () {
                    $(this).siblings('.sublist').slideToggle('slow');
                });
                $(catSel + ' > ul').slideToggle('slow');
            },
            error: this.ajaxFailure
        });
    },

    categoryList: function (data) {
        listItems = [];
        var self = this;
        for (i = 0; i < data.length; i++) {
            if (!data[i].IncludeInTopMenu) { continue; }
            listItems.push(self.categoryLine(data[i]));
         }
         return listItems;
    },

    categoryLine: function (data) {
        var self2 = this;

        let li = document.createElement('li');

        let a = document.createElement('a');
        a.href = data.Route;
        a.text = data.Name;

        li.appendChild(a);

        if (data.HaveSubCategories) {
            let div = document.createElement('div');
            div.classList.add('sublist-toggle');
            div.addEventListener("click",function(){ self2.getSubRoot(data.Id);});
            li.appendChild(div);
        }

        li.setAttribute(this.catLineAttr, data.Id);

        return li;
    },

    ajaxFailure: function () {
        alert('Failed to open menu. Please refresh the page and try one more time.');
    }
}

var topMenu = {
    rootRoute: '',
    subCatRoute: '',
    rootselector: '',

    init: function (rootRoute, subCatRoute, rootselector) {
        this.rootRoute = rootRoute;
        this.subCatRoute = subCatRoute;
        this.rootselector = $(rootselector).find('ul.top-menu:first-child');

        this.catLineAttr = 'catMenId';

        this.getRoot();
    },

    getRoot: function () {
        var self = this;
        $.ajax({
            cache: false,
            url: this.rootRoute,
            type: 'post',
            success: function (result) {
                var lis = self.categoryList(result);
                $(self.rootselector).append(lis);
            },
            error: this.ajaxFailure
        });
    },

    getSubRoot: function (id) {
        var self = this;

        var catSel = 'li[' + this.catLineAttr + ' = ' + id + ']';
        if ($(catSel).hasClass("loaded")) { return };

        $.ajax({
            cache: false,
            data: {
                "id": id,
            },
            url: this.subCatRoute,
            type: 'post',
            success: function (result) {
                var listItems = self.categoryList(result);
                if (listItems.length === 0) { $(catSel).addClass("loaded"); return; }
                let ul = document.createElement('ul');
                ul.classList.add('sublist');

                $(catSel).addClass("loaded");
                $(catSel).append(ul);

                $(catSel + ' > ul').append(listItems);
            },
            error: this.ajaxFailure
        });
    },

    categoryList: function (data) {
        listItems = [];
        var self = this;
        for (i = 0; i < data.length; i++) {
            if (!data[i].IncludeInTopMenu) { continue; }
            listItems.push(self.categoryLine(data[i]));
        }
        return listItems;
    },

    categoryLine: function (data) {
        var self = this;

        let li = document.createElement('li');

        let a = document.createElement('a');
        a.href = data.Route;
        a.text = data.Name;

        li.appendChild(a);

        if (data.HaveSubCategories) {
            let div = document.createElement('div');
            div.classList.add('sublist-toggle');
            li.appendChild(div);
            li.addEventListener("mouseenter", function () {
                $(this).addClass("inside");
                self.getSubRoot(data.Id);
            });
        }

        li.setAttribute(this.catLineAttr, data.Id);

        return li;
    },

    ajaxFailure: function () {
        alert('Failed to open menu. Please refresh the page and try one more time.');
    }
}
