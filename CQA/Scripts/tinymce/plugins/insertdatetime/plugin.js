/**
 * plugin.js
 *
 * Copyright, Moxiecode Systems AB
 * Released under LGPL License.
 *
 * License: http://www.tinymce.com/license
 * Contributing: http://www.tinymce.com/contributing
 */

/*global tinymce:true */

tinymce.PluginManager.add('insertdatetime', function(editor) {
	var daysShort = "Sun Mon Tue Wed Thu Fri Sat Sun".split(' ');
	var daysLong = "Sunday Monday Tuesday Wednesday Thursday Friday Saturday Sunday".split(' ');
	var monthsShort = "Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec".split(' ');
	var monthsLong = "January February March April May June July August September October November December".split(' ');
	var menuItems = [], lastFormat;

	function getDateTime(CQAt, date) {
		function addZeros(value, len) {
			value = "" + value;

			if (value.length < len) {
				for (var i = 0; i < (len - value.length); i++) {
					value = "0" + value;
				}
			}

			return value;
		}

		date = date || new Date();

		CQAt = CQAt.replace("%D", "%m/%d/%Y");
		CQAt = CQAt.replace("%r", "%I:%M:%S %p");
		CQAt = CQAt.replace("%Y", "" + date.getFullYear());
		CQAt = CQAt.replace("%y", "" + date.getYear());
		CQAt = CQAt.replace("%m", addZeros(date.getMonth() + 1, 2));
		CQAt = CQAt.replace("%d", addZeros(date.getDate(), 2));
		CQAt = CQAt.replace("%H", "" + addZeros(date.getHours(), 2));
		CQAt = CQAt.replace("%M", "" + addZeros(date.getMinutes(), 2));
		CQAt = CQAt.replace("%S", "" + addZeros(date.getSeconds(), 2));
		CQAt = CQAt.replace("%I", "" + ((date.getHours() + 11) % 12 + 1));
		CQAt = CQAt.replace("%p", "" + (date.getHours() < 12 ? "AM" : "PM"));
		CQAt = CQAt.replace("%B", "" + editor.translate(monthsLong[date.getMonth()]));
		CQAt = CQAt.replace("%b", "" + editor.translate(monthsShort[date.getMonth()]));
		CQAt = CQAt.replace("%A", "" + editor.translate(daysLong[date.getDay()]));
		CQAt = CQAt.replace("%a", "" + editor.translate(daysShort[date.getDay()]));
		CQAt = CQAt.replace("%%", "%");

		return CQAt;
	}

	function insertDateTime(format) {
		var html = getDateTime(format);

		if (editor.settings.insertdatetime_element) {
			var computerTime;

			if (/%[HMSIp]/.test(format)) {
				computerTime = getDateTime("%Y-%m-%dT%H:%M");
			} else {
				computerTime = getDateTime("%Y-%m-%d");
			}

			html = '<time datetime="' + computerTime + '">' + html + '</time>';

			var timeElm = editor.dom.getParent(editor.selection.getStart(), 'time');
			if (timeElm) {
				editor.dom.setOuterHTML(timeElm, html);
				return;
			}
		}

		editor.insertContent(html);
	}

	editor.addCommand('mceInsertDate', function() {
		insertDateTime(editor.getParam("insertdatetime_dateformat", editor.translate("%Y-%m-%d")));
	});

	editor.addCommand('mceInsertTime', function() {
		insertDateTime(editor.getParam("insertdatetime_timeformat", editor.translate('%H:%M:%S')));
	});

	editor.addButton('inserttime', {
		type: 'splitbutton',
		title: 'Insert time',
		onclick: function() {
			insertDateTime(lastFormat || "%H:%M:%S");
		},
		menu: menuItems
	});

	tinymce.each(editor.settings.insertdatetime_formats || [
		"%H:%M:%S",
		"%Y-%m-%d",
		"%I:%M:%S %p",
		"%D"
	], function(CQAt) {
		menuItems.push({
			text: getDateTime(CQAt),
			onclick: function() {
				lastFormat = CQAt;
				insertDateTime(CQAt);
			}
		});
	});

	editor.addMenuItem('insertdatetime', {
		icon: 'date',
		text: 'Insert date/time',
		menu: menuItems,
		context: 'insert'
	});
});
