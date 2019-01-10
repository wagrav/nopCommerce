var mainMenu = {
    rootRoute: '',
    subCatRoute: '',
    topMenuRootSelector: '',
    mobileMenuRootSelector: '',

    init: function (rootRoute, subCatRoute, topMenuRootSelector, mobileMenuRootSelector) {
        this.rootRoute = rootRoute;
        this.subCatRoute = subCatRoute;
        this.topMenuRootSelector = $(topMenuRootSelector);
        this.mobileMenuRootSelector = $(mobileMenuRootSelector);

        this.topMenuLineAttr = 'topMenuId';
        this.mobileMenuLineAttr = 'mobileMenuId';

        this.getRoot();
    },

    getRoot: function () {
        var self = this;
        $.ajax({
            cache: false,
            url: this.rootRoute,
            type: 'post',
            success: function (result) {
                var lis = self.categoryList(result, true);
                $(self.topMenuRootSelector).append(lis);

                lis = self.categoryList(result,false);
                $(self.mobileMenuRootSelector).append(lis);
            },
            error: this.ajaxFailure
        });
    },

    getSubRoot: function (id, isTopMenu) {
        var self = this;

        if (isTopMenu) {
            return self.getTopMenuSubRoot(id)
        }
        else {
            return self.getMobileMenuSubRoot(id)
        }
    },

    getTopMenuSubRoot: function (id) {
        var selfTop = this;

        var catSel = 'li[' + this.topMenuLineAttr + ' = ' + id + ']';
        if ($(catSel).hasClass("loaded")) { return };

        $.ajax({
            cache: false,
            data: {
                "id": id,
            },
            url: this.subCatRoute,
            type: 'post',
            success: function (result) {
                var listItems = selfTop.categoryList(result, true);
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

    getMobileMenuSubRoot: function (id) {
        var selfMobile = this;

        var catSel = 'li[' + this.mobileMenuLineAttr + ' = ' + id + ']';
        if ($(catSel).hasClass("loaded")) { return };

        $.ajax({
            cache: false,
            data: {
                "id": id,
            },
            url: this.subCatRoute,
            type: 'post',
            success: function (result) {
                var listItems = selfMobile.categoryList(result, false);
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

    categoryList: function (data, isTopMenu) {
        listItems = [];
        var self = this;
        for (i = 0; i < data.length; i++) {
            if (!data[i].IncludeInTopMenu) { continue; }
            listItems.push(self.categoryLine(data[i], isTopMenu));
        }
        return listItems;
    },

    categoryLine: function (data, isTopMenu) {
        var self = this;

        if (isTopMenu) {
            return self.topMenuCategoryLine(data)
        }
        else {
            return self.mobileMenuCategoryLine(data)
        }

    },

    topMenuCategoryLine: function (data) {
        var selfTop = this;

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
                selfTop.getSubRoot(data.Id, true);
            });
        }

        li.setAttribute(this.topMenuLineAttr, data.Id);

        return li;
    },

    mobileMenuCategoryLine: function (data) {
        var selfMobile = this;

        let li = document.createElement('li');

        let a = document.createElement('a');
        a.href = data.Route;
        a.text = data.Name;

        li.appendChild(a);

        if (data.HaveSubCategories) {
            let div = document.createElement('div');
            div.classList.add('sublist-toggle');
            div.addEventListener("click", function () { selfMobile.getSubRoot(data.Id, false); });
            li.appendChild(div);
        }

        li.setAttribute(this.mobileMenuLineAttr, data.Id);

        return li;
    },

    ajaxFailure: function () {
        alert('Failed to open menu. Please refresh the page and try one more time.');
    }
}
