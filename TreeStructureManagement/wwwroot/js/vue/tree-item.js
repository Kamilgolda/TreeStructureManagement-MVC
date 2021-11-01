//Compare items for sorting
function sortby(key) {
    return function innerSort(a, b) {
        if (!a.hasOwnProperty(key) || !b.hasOwnProperty(key)) return 0;
        const varA = (typeof a[key] === 'string') ? a[key].toUpperCase() : a[key];
        const varB = (typeof b[key] === 'string') ? b[key].toUpperCase() : b[key];
        let comparison = 0;
        if (varA > varB) comparison = 1;
            else if (varA < varB) comparison = -1;
        return comparison
    };
}

//Component of a single structure element
Vue.component("tree-item", {
    data() {
        return {
            isOpen: this.expand,
            children: this.item.children.$values,
            sortedChildren: []
        };
    },
    props: {
        item: Object,
        expand: Boolean,
        sortbyname: Boolean,
    },
    computed: {
        isNode: function () {
            return this.item.children.$values.length > 0
        },
    },
    created: function () {
        let clonedArray = JSON.parse(JSON.stringify(this.children))
        this.sortedChildren = clonedArray.sort(sortby('name'));
    },
    watch: {
        expand: function () {
            this.isOpen = this.expand;
        },
    },
    methods: {
        toggle: function () {
            if (this.isNode) {
                this.isOpen = !this.isOpen;
            }
        },
    },
    template: `
            <li>
                <div :class="{bold: isNode}" v-on:click="toggle">
                    <span v-if="isNode">[{{ isOpen ? '-' : '+' }}]</span>
                    <a :href="'/Management/Edit/'+item.id">{{item.name}}</a>
                </div>
                <ul v-show="isOpen" v-if="isNode && sortbyname == false">
                    <tree-item class="item" v-for="child in children" :key="child.id" :item="child" :expand="expand"></tree-item>
                </ul>
                <ul v-show="isOpen" v-if="isNode && sortbyname">
                    <tree-item class="item" v-for="child in sortedChildren" :key="child.id" :item="child" :expand="expand" :sortbyname="sortbyname"></tree-item>
                </ul>
            </li>
            `
});